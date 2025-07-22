using UnityEngine;
using XCharts.Runtime;
using LabJack;
using System.Threading;
using TMPro;
using System.IO;
using UnityEditor;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UI;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.SceneManagement;
using System.Globalization;

public class MVCManager : MonoBehaviour
{
    #region "Xchart Variables"
    [Header("XChart Variables")]
    public LineChart chart;
    public Serie serie;
    public int windowSizeMilisec = 5000; // Visible Data window in milliseconds

    public double serieMaxValue = 0; // Max Value contained in the serie data
    public double serieMinValue = 0; // min Value contained in the serie data

    public float serieLineWidth = 0.5f;

    #endregion

    #region "LabJack Variables"
    [Header("LabJack Variables")]
    public LabJackObject LJM;

    #endregion

    #region"PatientProfile Variables"

    #endregion

    #region "Data Storage Variables"
    public LabJackObject.LabJackDataPoint[][] MVCMeasurements;
    public LabJackObject.LabJackDataPoint[] MVC1;
    public LabJackObject.LabJackDataPoint[] MVC2;
    public LabJackObject.LabJackDataPoint[] MVC3;

    #endregion

    #region "Time Management Variables"
    [Header("Time Management Variables")]
    public float timeCounter = 0f;
    public double elapsedMillisec = 0; // Total time elapsed since the start of a reading loop (in Milli Seconds) 
    public float elapsedSec = 0;
    public float windowSizeSec = 0;

    #endregion

    #region "Coordination Variables"
    [Header("Coordination Variables")]
    public int currentIteration = 0;
    public int latestCheckedIteration = 0;

    public int serieLastIndex = 0;
    public int clipWindowEdgeSerieIndex = 0;

    #endregion

    #region "Testing Purpose Variables"
    [Header("Testing Purpose Variables")]
    public bool updatingChart = true;
    public bool useSlidingDataWindow = true;
    public bool useCustomClipChartMethod = true;
    public bool useSlidingXAxis = true;
    public int serieMaxCache = 5000;
    public int clipWindowMaxCache = 500;

    #endregion

    #region "Main Methods"

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LJM = Overseer.Instance.LJM;
        // LJM.InitializeAllValues();
        // LJM.ConnectLabJack();

        InitializeXChart();

        serie = chart.GetSerie(0);
        // if (useSlidingDataWindow)  
        // {
        //     //Set serie max cache based on the desired visibility window and the reading frequency
        //     double readingFreq = 1000000 / LJM.intervalReadingInMicroseconds;
        //     serieMaxCache = (int)Math.Round(windowSizeMilisec / 1000 * readingFreq, 0);
        //     serie.maxCache = serieMaxCache;
        // }
        if (useCustomClipChartMethod)  
        {
            //Set serie max cache based on the desired visibility window and the reading frequency
            double readingFreq = 1000000 / LJM.intervalReadingInMicroseconds;
            clipWindowMaxCache = (int)Math.Round(windowSizeMilisec / 1000 * readingFreq, 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //XCharts
        if (LJM.timerIsRunning && updatingChart)
        {
            //Debug.Log("Trying to Update Chart");
            UpdateChartAllMissingDataPoints();
        }
    }

    public void StartMVCMeasurement()
    {
        LJM.StartRecording();
    }

    public void StopMVCMeasurement()
    {
        LJM.StopRecording();
    }

    public void MVCProtocol()
    {
        // Optional: Display Instructions
        // First MVC measurement
        // Launch Recording 
        // Record in Chart
        // EndRecording
        // Record LJM dataArray as a MVCMeasurement
        int expectedMaxSize = (int)Math.Round(LJM.readingFreqHz * LJM.maxTimeReadLoopSec);//Size of the MVCMeasurements arrays
        int iterationMax = Math.Min(latestCheckedIteration, expectedMaxSize);

        MVCMeasurements = new LabJackObject.LabJackDataPoint[3][];
        MVCMeasurements[0] = new LabJackObject.LabJackDataPoint[expectedMaxSize];

        for (int i = 0; i < iterationMax; ++i)
        {
            MVCMeasurements[0][i] = LJM.dataArray[i]; // DataPoint is a struct so should not be an issue to copy as such
        }

        // Clean LJM dataArray
        LJM.CleanLabJackArray();


        // Press Next measurement
        // Or Press ReDo measurement
        // Or Press Skip Next

        // Second MVC measurement
        // Switch to measurement 2
        // Launch Recording 
        // Record in Chart
        // EndRecording
        // Record in dataArray

        expectedMaxSize = (int)Math.Round(LJM.readingFreqHz * LJM.maxTimeReadLoopSec);//Size of the MVCMeasurements arrays
        iterationMax = Math.Min(latestCheckedIteration, expectedMaxSize);

        for (int i = 0; i < iterationMax; ++i)
        {
            MVCMeasurements[1][i] = LJM.dataArray[i]; // DataPoint is a struct so should not be an issue to copy as such
        }

        // Press Next measurement
        // Or Press ReDo measurement
        // Or Press Skip Next

        // Third MVC measurement
        // Switch to measurement 3
        // Launch Recording 
        // Record in Chart
        // EndRecording
        // Record in dataArray
        expectedMaxSize = (int)Math.Round(LJM.readingFreqHz * LJM.maxTimeReadLoopSec);//Size of the MVCMeasurements arrays
        iterationMax = Math.Min(latestCheckedIteration, expectedMaxSize);

        for (int i = 0; i < iterationMax; ++i)
        {
            MVCMeasurements[2][i] = LJM.dataArray[i]; // DataPoint is a struct so should not be an issue to copy as such
        }

        // Press Visualize MVC
        // Display 3 measurements at once
        int numberOfMVCMeasurements = MVCMeasurements.Length;
        for (int i = 0; i < numberOfMVCMeasurements; ++i)
        {
            Serie MVCSerie = chart.series[i];
            MVCSerie.ClearData();
            foreach (var datapoint in MVCMeasurements[i])
            {
                AddTorqueDataPoint(datapoint, ref MVCSerie);
            }
        }
        // Display formula-calculated MVC
        // Excise quartiles 
        // Enter definitive MVC

        // Save MVC Value and 3 MVC measurements to patientA\sessionI folder 

    }

    #endregion

    #region "DataPoint Methods"

    // This method loops in Update() and progressively adds the data to the chart's Serie for visualisation
    public void UpdateChartAllMissingDataPoints()
    {
        currentIteration = LJM.latestIteration;

        while (latestCheckedIteration < currentIteration)
        {
            AddTorqueDataPoint(LJM.dataArray[latestCheckedIteration]);
            ++latestCheckedIteration;
        }
        latestCheckedIteration = currentIteration;
    }

    // Adds a dataPoint to the chart's serie AND updates the axis accordingly
    public void AddTorqueDataPoint(LabJackObject.LabJackDataPoint dataPoint, in DateTime startTime)
    {
        // 1. Get the time elapsed since start of reading loop
        elapsedMillisec = (dataPoint.time - startTime).TotalMilliseconds;

        // 2. Formatting and truncating to seconds
        elapsedSec = (float)elapsedMillisec / 1000;
        windowSizeSec = (float)windowSizeMilisec / 1000;


        // 3. Add Newly created Datapoint to chart's serie
        serie.AddData(elapsedSec, dataPoint.AIN0);

        // 4. Sliding X Window Methods

        UpdateXSlidingWindowAllMethods();

        // 5. Sliding Y Window Methods
        UpdateYSlidingWindow(dataPoint);

    }

    // Overrided Version with 1 parameter
    public void AddTorqueDataPoint(LabJackObject.LabJackDataPoint dataPoint)
    {
        AddTorqueDataPoint(dataPoint, LJM.startLoopTime);
    }

    // Override version allowing to precise which serie to update
    public void AddTorqueDataPoint(LabJackObject.LabJackDataPoint dataPoint, in DateTime startTime, ref Serie targetSerie)
    {
        // 1. Get the time elapsed since start of reading loop
        elapsedMillisec = (dataPoint.time - startTime).TotalMilliseconds;

        // 2. Formatting and truncating to seconds
        elapsedSec = (float)elapsedMillisec / 1000;
        windowSizeSec = (float)windowSizeMilisec / 1000;


        // 3. Add Newly created Datapoint to chart's serie
        targetSerie.AddData(elapsedSec, dataPoint.AIN0);

        // 4. Sliding X Window Methods

        UpdateXSlidingWindowAllMethods();

        // 5. Sliding Y Window Methods
        UpdateYSlidingWindow(dataPoint);
    }

    public void AddTorqueDataPoint(LabJackObject.LabJackDataPoint dataPoint, ref Serie targetSerie)
    {
        AddTorqueDataPoint(dataPoint, LJM.startLoopTime, ref targetSerie);
    }

    #endregion
    #region "X Sliding Window Methods"
    public void UpdateXSlidingWindowAllMethods()
    {
        // 4.1 Sliding Window with serie.MaxCache 
        // Keep data size at a certain point, should be automatically done by Xchart
        if (useSlidingDataWindow && serie.dataCount > serieMaxCache)
        {
            UpdateXSlidingWindowMaxCache();
        }

        // 4.2 Sliding XAxis with Time
        // Updating XAxis to match the data and the selected window size
        if (useSlidingXAxis)
        {
            UpdateXSlidingWindowSlidingAxis();
        }

        //4.3 Sliding Window with Custom "Clip" Method 
        // Bug: datapoints stay ignored even after clearing the chart
        if (useCustomClipChartMethod)
        {
            UpdateXSlidingWindowCustomClipMethod();
        }
    }

    public void UpdateXSlidingWindowMaxCache()
    {
        if (serie.dataCount > serieMaxCache)
        {
            serie.RemoveData(0);
        }
    }

    public void UpdateXSlidingWindowSlidingAxis()
    {
        var xAxis = chart.GetChartComponent<XAxis>();
        // Axis type has to be custon to directly modify the min and max values
        xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        xAxis.max = Math.Round(elapsedSec, 2);
        xAxis.min = Math.Round(elapsedSec - windowSizeSec, 2);
    }

    public void UpdateXSlidingWindowCustomClipMethod() //Original Clip Boolean seems to work better, this function could be retired
    {
        serieLastIndex = serie.dataCount - 1;
        if (serieLastIndex > clipWindowMaxCache)
        {
            clipWindowEdgeSerieIndex = serieLastIndex - clipWindowMaxCache-1; // -1 added as method is called after adding the latest datapoint
            serie.data[clipWindowEdgeSerieIndex].ignore = true;
        }  
    }

    #endregion
    #region "Y SlidingWindow Methods"

    public void UpdateYSlidingWindow(LabJackObject.LabJackDataPoint dataPoint)
    {
        // 5.1 Updating the Yaxis borders to match the max value
        if (dataPoint.AIN0 > serieMaxValue)
        {
            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;

            serieMaxValue = dataPoint.AIN0;

            yAxis.max = Math.Round(serieMaxValue, 2);
        }
        // 5.2 Updating the Yaxis borders to match the min value
        else if (dataPoint.AIN0 < serieMinValue)
        {
            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;

            serieMinValue = dataPoint.AIN0;

            yAxis.min = Math.Round(serieMinValue, 2);
        }
    }
    #endregion

    #region "LabJack Helper Methods"
    public void ConnectLabJack()
    {
        LJM.ConnectLabJack();
    }

    public void DisconnectLabJack()
    {
        LJM.DisconnectLabJack();
    }

    #endregion
    #region "XChart Helper Methods"
    public void ClearGraph()
    {
        SerieUnIgnoreAllData(ref serie);
        chart.ClearData();
    }

    public void SwapSerieTo(int serieIndex)
    {
        serie.show = false;
        serie = chart.GetSerie(serieIndex);
        serie.show = true;

        serie.maxCache = serieMaxCache;

        // Optional: customize serie style
        serie.lineStyle.width = serieLineWidth; 
    }

    public void SerieUnIgnoreAllData(ref Serie  serie)
    {
        int serieSize = serie.dataCount; 
        for (int i = 0; i<serieSize; ++i)
        {
            serie.data[i].ignore = false;
        }
    }
    public void SerieUnIgnoreAllData()
    {
        SerieUnIgnoreAllData(ref serie);        
    }

    #endregion

    #region "Initialisation Methods"
    // public void ConnectLabJack()
    // {
    //     //Open first found LabJack
    //     LJM.OpenS("ANY", "ANY", "ANY", ref handle);

    //     // Get and Display Device Info
    //     LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
    //     LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
    //     // Converts numeric IP to a readable string.
    //     LJM.NumberToIP(ipAddr, ref ipAddrStr);

    //     Debug.Log("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");
    //     Debug.Log("  Serial number: " + serNum + ", IP address: " + ipAddrStr + ", Port: " + port + ",");
    //     Debug.Log("  Max bytes per MB: " + maxBytesPerMB);
    //     isLabJackConnected = true;

    // }

    public void InitializeXChart()
    {
        // Set Chart Title
        chart.EnsureChartComponent<Title>().text = "Real-Time Torque Data";

        // Initialize and clear serie data
        chart.ClearData();

        // Optional: Add named serie
        //chart.AddSerie<Line>("Torque");

        // Optional: Customize appearance
        chart.GetSerie(0).symbol.show = false;

        // Initialise serie
        serie = chart.GetSerie(0);
        serie.maxCache = serieMaxCache;

        // Optional: customize serie style
        serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin
    }

    public void InitializeSerie()
    {
        serie.maxCache = serieMaxCache;

        // Optional: customize serie style
        serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin
    }
    #endregion

    #region "Testing Purpose Methods"

    public void InitializeLabJack()
    {
        LJM.InitializeAllValues();
        LJM.ConnectLabJack();
    }


    #endregion

}
