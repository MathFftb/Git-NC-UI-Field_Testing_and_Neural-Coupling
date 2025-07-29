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
using System.Collections;
using System.Data;
using System.Linq;

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
    public LabJackObject.LabJackDataPoint[][] MVCMeasurements; // Stores all the datapoints to be saved at the end of the protocol
    public LabJackObject.LabJackDataPoint[] MVC1;
    public LabJackObject.LabJackDataPoint[] MVC2;
    public LabJackObject.LabJackDataPoint[] MVC3;
    public double MVCValueFinal;

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

    public bool isReading = false;

    #endregion

    #region "MVC User Flow Variables"
    public int MVCCount = 0; // First measurement is number 0, there are 3 measurements
    public int maxNumberMVCMeasurements = 3;
    #endregion

    #region "User Input Variables"
    public double userInputMaxTimeReadLoopSec = 10;
    #endregion

    #region "UI Elements"
    [Header("UI Elements")]
    public AltOpenCloseWindow FirstWindow;
    public AltOpenCloseWindow MeasurementWindow;
    public AltOpenCloseWindow lastMeasurementEndWindow;

    public Text MeasurementCountDisplay;
    public TMP_InputField MaxDurationInputField;
    public Button StartMeasurementButton;
    public Button SkipMeasurementButton;

    public Image MVCInstructions;
    public bool displayInstructions = true; // Instructions should not have to be displayed every time
    public Button GoStopDisplay;
    public Button StopReadLoopButton;

    public Button SaveRecentMVCMeasurementButton;
    public Button RedoRecentMVCMeasurementButton;

    public TMP_InputField MVCValueInputField;
    public Button SaveAllMVCMeasurementsToFile;

    #endregion

    #region "Testing Purpose Variables"
    [Header("Testing Purpose Variables")]
    public bool testing = false;
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

        InitializeMVCUI();

        LJM.OnLabJackReadingStart.AddListener(OnReadingStart);
        LJM.OnLabJackReadingEnd.AddListener(OnReadingEnd);

        InitializeMVCMeasurements();

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

    public IEnumerator StartMVCMeasurement()
    {
        // Warning: IEnumerator cannot be called from buttons
        yield return StartMVCMeasurementUIAnimation();
        LJM.StartRecording();
    }

    public void OnClickStartMVCMeasurement()
    {
        StartCoroutine(StartMVCMeasurement());
    }

    public IEnumerator StartMVCMeasurementUIAnimation()
    {
        // Display a countdown before starting the MVC Measurement
        GoStopDisplay.gameObject.SetActive(true);
        GoStopDisplay.GetComponentInChildren<TMP_Text>().text = "3";
        yield return new WaitForSeconds(1);
        GoStopDisplay.GetComponentInChildren<TMP_Text>().text = "2";
        yield return new WaitForSeconds(1);
        GoStopDisplay.GetComponentInChildren<TMP_Text>().text = "1";
        yield return new WaitForSeconds(1);
        GoStopDisplay.GetComponentInChildren<TMP_Text>().text = "GO!";
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
        // Enter final MVC value

        // Save MVC Value and 3 MVC measurements to patientA\sessionI folder 

    }

    #endregion

    #region "DataPoint Methods"

    /// <summary>
    /// Loops in Update() and progressively adds the data to the chart's Serie for visualisation
    /// </summary>
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

    /// <summary>
    /// Adds a dataPoint to the chart's serie AND updates the axis accordingly
    /// </summary>
    /// <param name="dataPoint"></param>
    /// <param name="startTime"></param>
    public void AddTorqueDataPoint(LabJackObject.LabJackDataPoint dataPoint, in DateTime startTime, bool slidingXWindow = true, bool slidingYWindow = true)
    {
        // 1. Get the time elapsed since start of reading loop
        elapsedMillisec = (dataPoint.time - startTime).TotalMilliseconds;

        // 2. Formatting and truncating to seconds
        elapsedSec = (float)elapsedMillisec / 1000;
        windowSizeSec = (float)windowSizeMilisec / 1000;


        // 3. Add Newly created Datapoint to chart's serie
        serie.AddData(elapsedSec, dataPoint.AIN0);

        // 4. Sliding X Window Methods

        if (slidingXWindow)
            UpdateXSlidingWindowAllMethods();

        // 5. Sliding Y Window Methods
        if (slidingYWindow)
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

    public void UpdateXSlidingWindowCustomClipMethod() //Original Xcharts Clip option (Boolean) seems to work better, this function could be retired
    {
        serieLastIndex = serie.dataCount - 1;
        if (serieLastIndex > clipWindowMaxCache)
        {
            clipWindowEdgeSerieIndex = serieLastIndex - clipWindowMaxCache - 1; // -1 added as method is called after adding the latest datapoint
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

    /// <summary>
    /// Swaps the active Serie Chart Index
    /// </summary>
    public void SwapSerieTo(int serieIndex)
    {
        serie.show = false;

        // Make sure the serie exists
        while (chart.series.Count <= serieIndex)
        {
            chart.AddSerie<Line>($"MVC{MVCCount + 1}");
        }

        // Swap active serie
        serie = chart.GetSerie(serieIndex);
        serie.show = true;

        serie.maxCache = serieMaxCache;

        // Optional: customize serie style
        serie.lineStyle.width = serieLineWidth;
        serie.clip = true;
        serie.symbol.show = false;
    }

    public void SerieUnIgnoreAllData(ref Serie serie)
    {
        int serieSize = serie.dataCount;
        for (int i = 0; i < serieSize; ++i)
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
        serie.symbol.show = false;
    }

    public void InitializeSerie() // Initilizes the active Serie object "serie"
    {
        serie.maxCache = serieMaxCache;

        // Optional: customize serie style
        serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin
        serie.symbol.show = false;
    }

    public void InitializeMVCMeasurements() // Initializes the arrays to be saved at the end
    {
        MVCMeasurements = new LabJackObject.LabJackDataPoint[maxNumberMVCMeasurements][];
    }



    #endregion

    #region "Testing Purpose Methods"

    public void InitializeLabJack()
    {
        LJM.InitializeAllValues();
        LJM.ConnectLabJack();
    }


    #endregion

    #region "MVC Flow Management Method"
    public void NextMVCMeasurement()
    {
        if (MVCCount < maxNumberMVCMeasurements - 1)
        {
            // Update MVC Count
            MVCCount++;
            // Swap active serie 
            SwapSerieTo(MVCCount);
            // Update UI interactability
            OnPrepareReadLoopUI();
            // Change MVC Count Display
            UpdateMeasurementCountDisplay();
        }
        else
        {
            OnLastMeasurementEnd();
        }
    }

    public void SaveCurrentMVC()
    {
        int expectedMaxSize = (int)Math.Round(LJM.readingFreqHz * LJM.maxTimeReadLoopSec);//Size of the MVCMeasurements arrays
        int iterationMax = Math.Min(latestCheckedIteration, expectedMaxSize);

        MVCMeasurements[MVCCount] = new LabJackObject.LabJackDataPoint[expectedMaxSize];

        for (int i = 0; i < iterationMax; ++i)
        {
            MVCMeasurements[MVCCount][i] = LJM.dataArray[i]; // DataPoint is a struct so should not be an issue to copy as such
        }
    }

    public void SaveMVCToFile()
    {
        // Get the User determined MVC Value
        OnMVCValueUserInputChanged();
        // Save the MVC Value to the Overseer
        Overseer.Instance.MVCValue = MVCValueFinal;
        // Save the MVC Value to the Session Info File as Json

        // Save the MVC Measurements as separate csv files in the session folder
        // Note: Csv method creates the files if they are not created
        for (int i = 0; i < MVCMeasurements.Length; ++i)
        {
            // Set the path to the savefile
            Overseer.Instance.currentMVCMeasurementFileName = $"MVCMeasurement{i + 1}.csv";
            Overseer.Instance.UpdateAllPaths();
            string filePath = Overseer.Instance.currentMVCMeasurementFilePath;
            // Save the data in the file
            CsvConverter.SaveAsCsv(MVCMeasurements[i], dp => dp.ToCsv(), filePath, LabJackObject.LabJackDataPoint.CsvHeader());
        }
    }

    public void ReDoLastMVCMeasurement()
    {
        // // Do Not Update MVC Count
        // MVCCount = MVCCount;
        // // Swap active serie 
        // SwapSerieTo(MVCCount);
        // // Change MVC Count Display
        // UpdateMeasurementCountDisplay();

        // Clear current serie
        serie.ClearData();

    }

    #endregion

    #region "User Input Methods"
    public void OnMaxDurationChanged()
    {
        if (MaxDurationInputField.text != "")
        {
            userInputMaxTimeReadLoopSec = double.Parse(MaxDurationInputField.text);
            LJM.maxTimeReadLoopSec = userInputMaxTimeReadLoopSec;
        }
    }

    public void OnMVCValueUserInputChanged()
    {
        if (MVCValueInputField.text != "")
        {
            MVCValueFinal = double.Parse(MVCValueInputField.text);
            SaveAllMVCMeasurementsToFile.interactable = true;
        }
        else
        {
            SaveAllMVCMeasurementsToFile.interactable = false;
        }
    }
    #endregion

    #region "UI Methods"
    public void InitializeMVCUI()
    {
        if (!testing) MaxDurationInputField.text = LJM.maxTimeReadLoopSec.ToString();

        // Initialize Windows Canvas
        FirstWindow.OpenWindow();
        // MeasurementWindow.CloseWindow();
        // lastMeasurementEndWindow.CloseWindow();

        // Initialize measurement count display
        UpdateMeasurementCountDisplay();


    }

    public void ActivateReadingModeUI()
    {
        isReading = true;
        UpdateReadingModeUI();
    }

    public void DeactivateReadingModeUI()
    {
        Debug.Log("Caught End event");
        isReading = false;
        UpdateReadingModeUI();
        Debug.Log($"DeactivateReadingModeUI() running on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
    }

    public void UpdateReadingModeUI()
    {
        MaxDurationInputField.interactable = !isReading;
        StartMeasurementButton.interactable = !isReading;
        SkipMeasurementButton.interactable = !isReading;

        StopReadLoopButton.interactable = isReading;
        GoStopDisplay.gameObject.SetActive(isReading);

        SaveRecentMVCMeasurementButton.interactable = !isReading;
        RedoRecentMVCMeasurementButton.interactable = !isReading;
    }

    public void OnPrepareReadLoopUI()
    {
        // Update UI Elements interactibility

        MeasurementCountDisplay.enabled = true;
        MaxDurationInputField.interactable = true;
        StartMeasurementButton.interactable = true;
        SkipMeasurementButton.interactable = false;

        MVCInstructions.gameObject.SetActive(false);
        displayInstructions = false; // Instructions should not have to be displayed every time
        GoStopDisplay.gameObject.SetActive(true);
        // GoStopDisplay.Text = "GO!";
        StopReadLoopButton.interactable = false;

        SaveRecentMVCMeasurementButton.interactable = false;
        RedoRecentMVCMeasurementButton.interactable = false;

        MVCValueInputField.interactable = false;
        SaveAllMVCMeasurementsToFile.interactable = false;
    }

    public void OnReadingStart()
    {
        // Update UI Elements interactibility

        MeasurementCountDisplay.enabled = true;
        MaxDurationInputField.interactable = false;
        StartMeasurementButton.interactable = false;
        SkipMeasurementButton.interactable = false;

        MVCInstructions.gameObject.SetActive(false);
        displayInstructions = false; // Instructions should not have to be displayed every time
        GoStopDisplay.gameObject.SetActive(true);
        // GoStopDisplay.Text = "GO!";
        StopReadLoopButton.interactable = true;

        SaveRecentMVCMeasurementButton.interactable = false;
        RedoRecentMVCMeasurementButton.interactable = false;

        MVCValueInputField.interactable = false;
        SaveAllMVCMeasurementsToFile.interactable = false;
    }

    public void OnReadingEnd()
    {
        Debug.Log("Caught End event");
        isReading = false;
        // Update UI Elements interactibility

        MeasurementCountDisplay.enabled = true;
        MaxDurationInputField.interactable = false;
        StartMeasurementButton.interactable = false;
        SkipMeasurementButton.interactable = false;

        MVCInstructions.gameObject.SetActive(false);
        displayInstructions = false; // Instructions should not have to be displayed every time
        GoStopDisplay.gameObject.SetActive(true);
        GoStopDisplay.GetComponentInChildren<TMP_Text>().text = "Relax";
        StopReadLoopButton.interactable = false;

        SaveRecentMVCMeasurementButton.interactable = true;
        RedoRecentMVCMeasurementButton.interactable = true;

        MVCValueInputField.interactable = false;
        SaveAllMVCMeasurementsToFile.interactable = false;

        Debug.Log($"DeactivateReadingModeUI() running on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");

    }

    public void OnLastMeasurementEnd()
    {
        // Update UI Elements interactibility

        MeasurementCountDisplay.enabled = true;
        MaxDurationInputField.interactable = false;
        StartMeasurementButton.interactable = false;
        SkipMeasurementButton.interactable = false;

        MVCInstructions.gameObject.SetActive(false);
        displayInstructions = false; // Instructions should not have to be displayed every time
        GoStopDisplay.gameObject.SetActive(false);
        // GoStopDisplay.Text = "GO!";
        StopReadLoopButton.interactable = false;

        SaveRecentMVCMeasurementButton.interactable = false;
        RedoRecentMVCMeasurementButton.interactable = false;

        // Make sure the final window is open
        lastMeasurementEndWindow.OpenWindow();
        MVCValueInputField.enabled = true;
        SaveAllMVCMeasurementsToFile.enabled = true;
        MVCValueInputField.interactable = true;
        SaveAllMVCMeasurementsToFile.interactable = false; // becomes interactable when MVC has been selected

        // Display all data from the MVCMeasurements arrays []
        double maxDuration = 0;
        for (int i = 0; i < MVCCount; ++i)
        {
            if (chart.series.Count > MVCCount && MVCMeasurements[i].Length > 0)
            {
                serie = chart.GetSerie(i);
                serie.ClearData(); // Clear data just in case, we assume any modification could have happened to the original serie
                serie.maxCache = MVCMeasurements[i].Length;

                // Style option of the serie 
                serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin
                serie.symbol.show = false;
                serie.show = true;

                // Get MVC Measurement start 
                DateTime readStartTime = MVCMeasurements[i][0].time;
                // Get MVC Duration in seconds
                double durationSec = (float)(MVCMeasurements[i][^1].time - MVCMeasurements[i][0].time).TotalMilliseconds / 1000;

                foreach (var datapoint in MVCMeasurements[i])
                {
                    AddTorqueDataPoint(datapoint, readStartTime, slidingXWindow: false, slidingYWindow: true);
                }

                // Adjust the X axis to show all data
                if (durationSec > maxDuration)
                {
                    if (AppSettings.DebugMode) Debug.Log($"New Max Duration set: {maxDuration}");
                    maxDuration = durationSec;
                    var xAxis = chart.GetChartComponent<XAxis>();
                    // Axis type has to be custon to directly modify the min and max values
                    xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
                    xAxis.max = Math.Round(maxDuration, 2);
                    xAxis.min = Math.Round(-0.5, 2);
                }

            }
        }
    }

    public void UpdateMeasurementCountDisplay()
    {
        MeasurementCountDisplay.text = $"Measurement #{MVCCount + 1}";
    }

    #endregion

    #region "Calculating MVC"

    struct MVCCandidate
    {
        public LabJackObject.LabJackDataPoint datapoint;
        public bool isMax;
        public bool isMin;
        public MVCCandidate(LabJackObject.LabJackDataPoint dp, bool max = false, bool min = false)
        {
            datapoint = dp;
            isMax = max;
            isMin = min;
        }
    }
    
    public double GetMVCUsingIndexing(LabJackObject.LabJackDataPoint[] dataset)
    {
        double finalMVC = 0;
        double _windowWidthInSec = 10;
        int _windowWidthInDatapoints = (int)Math.Round(LJM.readingFreqHz * _windowWidthInSec);

        (int start, int stop) window;
        // Initialize Window
        window.start = 0;
        window.stop = _windowWidthInDatapoints;
        
        var comparedValue = new LabJackObject.LabJackDataPoint();
        (LabJackObject.LabJackDataPoint datapoint, int index) maxInWindow = (new LabJackObject.LabJackDataPoint(), 0);
        (LabJackObject.LabJackDataPoint datapoint, int index) minInWindow = (new LabJackObject.LabJackDataPoint(), 0);
        for (int i = window.start; i < window.stop; ++i)
        {
            comparedValue = dataset[i];
            // Define max value
            if (dataset[i].CompareTo(maxInWindow.datapoint) > 0)
            {
                maxInWindow = (comparedValue, index: i);
            }
            // Define min value
            else if (dataset[i].CompareTo(minInWindow.datapoint) < 0)
            {
                minInWindow = (comparedValue, index: i);
            }
        }

        double stabilityTolerance = 2;
        // Does not need indexing here
        double comparedMVC;
        double maxMVCYet = 0;


        // Loop over the rest of the dataset 
        for (int i = _windowWidthInDatapoints; i < dataset.Length; ++i)
        {
            

            // Update the Window
            window.start = i - _windowWidthInDatapoints;
            window.stop = i;

            // Check if disappearing part of the window means we need an update
            // If maxInWindow is out of the window
            if (maxInWindow.index < window.start)
            {
                // Get the new max
                for (int j = window.start; j < window.stop; i++)
                {
                    if (dataset[j].CompareTo(maxInWindow.datapoint) > 0)
                    {
                        maxInWindow = (dataset[j], index: j);
                    }
                }
            }
            // If minInWindow is out of the window
            else if (minInWindow.index < window.start)
            {
                // Get the new min
                for (int j = window.start; j < window.stop; i++)
                {
                    if (dataset[j].CompareTo(minInWindow.datapoint) < 0)
                    {
                        minInWindow = (dataset[j], index: j);
                    }
                }
            }

            // Check if the new datapoints entering the window are the new min or max
            // Check if there is a new max 
            if (dataset[window.stop].CompareTo(maxInWindow.datapoint) > 0)
            {
                // Update maxInWindow
                maxInWindow = (dataset[window.stop], window.stop);
            }
            // Check if there is a new minif
            else if (dataset[window.stop].CompareTo(minInWindow.datapoint) < 0)
            {
                // Update minInWindow
                minInWindow = (dataset[window.stop], window.stop);
            }
            

            // Check if the window is eligible for MVC
            if (maxInWindow.datapoint.AIN0 - minInWindow.datapoint.AIN0 < stabilityTolerance)
            {
                // Register the current MVC
                comparedMVC = maxInWindow.datapoint.AIN0;
                // Chek if the new MVC is higher than maxMVCyet 
                if (comparedMVC > maxMVCYet)
                {
                    // Update maxMVC
                    maxMVCYet = comparedMVC;
                }
            }
        }
        finalMVC = maxMVCYet; 
        return finalMVC;
    }

    // public double GetMVCUsingQueue(LabJackObject.LabJackDataPoint[] dataset)
    // {
    //     double finalMVC = 0;
    //     double _windowWidthInSec = 10;
    //     int _windowWidthInDatapoints = (int)Math.Round(LJM.readingFreqHz * _windowWidthInSec);

    //     var maxInWindow = new LabJackObject.LabJackDataPoint();
    //     var minInWindow = new LabJackObject.LabJackDataPoint();
    //     var comparedValue = new LabJackObject.LabJackDataPoint();

    //     Queue<MVCCandidate> timeWindow = new Queue<MVCCandidate>();
    //     // Initialize Window
    //     for (int i = 0; i < _windowWidthInDatapoints; ++i)
    //     {
    //         // Update max or min in window
    //         comparedValue = dataset[i];
    //         if (comparedValue.CompareTo(maxInWindow) > 0)
    //         {
    //             timeWindow.Enqueue(new MVCCandidate(dataset[i], max: true, min: false));
    //             maxInWindow = comparedValue;
    //         }
    //         else if (comparedValue.CompareTo(minInWindow) < 0)
    //         {
    //             timeWindow.Enqueue(new MVCCandidate(dataset[i], max: false, min: true));
    //             minInWindow = comparedValue;
    //         }
    //         else
    //         {
    //             timeWindow.Enqueue(new MVCCandidate(dataset[i], max: false, min: false));
    //         }
    //     }

    //     double  maxMVCYet = 0;
    //     double comparedMVC;
    //     double variationMinMax = 0;
    //     double maxVariationTolerance = 2;
    //     bool usingAverage = true;
    //     bool usingMaxValue = true;



    //     // Loop over the rest of the dataset
    //     for (int i = _windowWidthInDatapoints; i < dataset.Length; ++i)
    //     {
    //         // Check if the window is stable
    //         if (maxInWindow.AIN0 - minInWindow.AIN0 < maxVariationTolerance)
    //         {
    //             // Check if MVC needs to be updated (2 methods)
    //             if (usingAverage)
    //             {
    //                 // Check if the average of the window is higher than previous MVC registered
    //                 comparedMVC = timeWindow.
    //                             OfType<LabJackObject.LabJackDataPoint>().
    //                                 Average(datapoint => datapoint.AIN0);
    //                 if (comparedMVC > maxMVCYet)
    //                 {
    //                     maxMVCYet = comparedMVC;
    //                 }
    //             }
    //             else if (usingMaxValue)
    //             {
    //                 // Check if the max value in the window is higher than previous MVC registered
    //                 comparedMVC = maxInWindow.AIN0;
    //                 if (comparedMVC > maxMVCYet)
    //                 {
    //                     maxMVCYet = comparedMVC;
    //                 }
    //             }
    //         }
    //         // Enqueue
    //         // Update max or min in window
    //         comparedValue = dataset[i];
    //         if (comparedValue.CompareTo(maxInWindow) > 0)
    //         {
    //             timeWindow.Enqueue(new MVCCandidate(dataset[i], max: true, min: false));
    //             maxInWindow = comparedValue;
    //         }
    //         else if (comparedValue.CompareTo(minInWindow) < 0)
    //         {
    //             timeWindow.Enqueue(new MVCCandidate(dataset[i], max: false, min: true));
    //             minInWindow = comparedValue;
    //         }
    //         else
    //         {
    //             timeWindow.Enqueue(new MVCCandidate(dataset[i], max: false, min: false));
    //         }

    //         // Dequeue
    //         var leavingCandidate = (MVCCandidate)timeWindow.Dequeue();
    //         // Examine the leaving value and act in accordance

    //         if (leavingCandidate.isMax)
    //         {
    //             // Find the new maxInWindow
    //             //var thing = timeWindow.Max(candidate => candidate.datapoint).isMax = true;
    //             // Simple Loop
    //             var temporaryMaxDatapoint = new LabJackObject.LabJackDataPoint();

    //             foreach (var candidate in timeWindow)
    //             {
    //                 if (candidate.datapoint.CompareTo(temporaryMaxDatapoint) > 0)
    //                 {
    //                     temporaryMaxDatapoint = candidate.datapoint;

    //                 }
    //             }

    //         }
    //         else if (leavingCandidate.isMin)
    //         {
    //             // Find the new min

    //         }
    //     }




    //     return finalMVC;
    // }



    #endregion

}
