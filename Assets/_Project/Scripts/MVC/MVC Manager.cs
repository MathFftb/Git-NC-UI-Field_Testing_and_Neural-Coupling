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

    #region "Time Management Variables"
    [Header("Time Management Variables")]
    public float timeCounter = 0f;
    public double elapsedMillisec = 0; // Total time elapsed since the start of a reading loop (in Milli Seconds) 

    #endregion

    #region "Coordination Variables"
    [Header("Coordination Variables")]
    public int currentIteration = 0;
    public int latestCheckedIteration = 0;


    #endregion

    #region "Testing Purpose Variables"
    [Header("Testing Purpose Variables")]
    public bool updatingChart = true;
    public bool useSlidingWindow = true;
    public bool useSlidingXAxis = true;
    public int serieMaxCache = 5000;

    #endregion

    #region "Main Methods"

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LJM = Overseer.Instance.LJM;
        LJM.InitializeAllValues();
        LJM.ConnectLabJack();

        InitializeXChart();

        //Set serie max cache based on the desired visibility window and the reading frequency
        serie = chart.GetSerie(0);
        double readingFreq = 1000000 / LJM.intervalReadingInMicroseconds;
        serieMaxCache = (int)Math.Round(windowSizeMilisec / 1000 * readingFreq, 0); 
        serie.maxCache = serieMaxCache;

    }

    // Update is called once per frame
    void Update()
    {
        // LabJack
        if (LJM.timerIsRunning)
        {
            LJM.timeReadingSec += Time.deltaTime;
            if (LJM.timeReadingSec > LJM.maxTimeReadLoopSec)
                LJM.timerIsRunning = false;
        }

        //XCharts
        if (LJM.timerIsRunning && updatingChart)
        {
            Debug.Log("Trying to Update Chart");
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
        // Get the time elapsed since start of reading loop
        elapsedMillisec = (dataPoint.time - startTime).TotalMilliseconds;

        // Formatting and truncating to seconds
        float elapsedSec = (float)elapsedMillisec / 1000;
        float windowSizeSec = (float)windowSizeMilisec / 1000;
        

        // Add Newly created Datapoint to chart's serie
        serie.AddData(elapsedSec, dataPoint.AIN0);

        // Keep data size at a certain point, should be automatically done by Xchart
        if (useSlidingWindow && serie.dataCount > serieMaxCache)
        {
            serie.RemoveData(0);
        }

        // Updating XAxis to match the data and the selected window size
        if (useSlidingXAxis)
        {
            var xAxis = chart.GetChartComponent<XAxis>();

            // Axis type has to be custon to directly modify the min and max values
            xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
            xAxis.max = Math.Round(elapsedSec, 2);
            xAxis.min = Math.Round(elapsedSec - windowSizeSec, 2);
        }

        // Updating the Yaxis borders to match the max value
        if (dataPoint.AIN0 > serieMaxValue)
        {
            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;

            serieMaxValue = dataPoint.AIN0;

            yAxis.max = Math.Round(serieMaxValue, 2);
        }
        // Updating the Yaxis borders to match the min value
        else if (dataPoint.AIN0 < serieMinValue)
        {
            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;

            serieMinValue = dataPoint.AIN0;

            yAxis.min = Math.Round(serieMinValue, 2);
        }

    }

    // Overrided Version with 1 parameter
    public void AddTorqueDataPoint(LabJackObject.LabJackDataPoint dataPoint)
    {
        AddTorqueDataPoint(dataPoint, LJM.startLoopTime);
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
        chart.ClearData();
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
    #endregion

    #region "Inspector Testing Methods"

    #endregion

}
