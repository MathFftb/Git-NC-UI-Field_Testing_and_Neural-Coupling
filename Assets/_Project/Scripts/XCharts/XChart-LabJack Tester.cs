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
public class XChartLabJackTester : MonoBehaviour
{
    #region "XChart Variables"
    [Header("XCharts Variables")]
    public LineChart chart;
    public Serie serie;

    public int serieMaxCache = 500;
    public float serieLineWidth = 0.5f; 

    public double aValue;

    private float timeCounter = 0f;

    public bool UpdateOneByOne = true;
    public bool updatingChart = true;
    public Toggle UpdateOneByOneToggle;
    public Toggle updatingChartInput;

    public int maxDataPoints = 100;



    #endregion

    #region "LabJack Variables"
    [Header("LabJack Variables")]
    public bool isRunning = false;
    public bool isRecording = false;
    public bool isConnected = false;

    public bool recordWithDateTime = false;
    public Toggle recordWithDateTimeInput;

    private double elapsedMs = 0;

    
    int handle = 0;
    int devType = 0; // Device type (T4, T7, T8)
    int conType = 0; // USB, Ethernet, WiFi
    int serNum = 0;  // Serial number
    int ipAddr = 0;  // IP address (numeric form)
    string ipAddrStr = "";
    int port = 0;
    int maxBytesPerMB = 0;

    int errorAddress = -1;

    int intervalHandle = 1; // For timed reading every second

    int numFrames = 0;
    string[] aNames; // array of strings there to be filled and passed for command in this code
    double[] aValues; // array of doubles there to be filled and passed for command in this code

    int skippedIntervals = 0;

    

    private Thread readThread;

    public int MaxIterations = 30;

    public int IntervalReadingInMicroseconds = 1000000;
    public TMP_InputField ReadingIntervalInput;

    private double[] bufferToSave;
    public int sizeOfBufferToSave = 100000000;

    public string bufferCsvFormatted;

    public double maxTimeReadLoop = 10;
    public double timeReading;
    public bool timerIsRunning;
    public TMP_InputField maxTimeUserInput;

    public int totalSkippedIntervals = 0;



    [Serializable]
    public struct DataPoint
    {
        public double AIN0;
        public DateTime time;

        public override string ToString()
        {
            return $"AIN0 = {AIN0}\n";
        }

        public string ToCsv()
        {
            return $"{AIN0},";
        }
        public static string CsvHeader()
        {
            return "AIN0";
        }
    }

    [SerializeField]
    private DataPoint latestDataPoint;
    [SerializeField]
    private CircularBuffer<DataPoint> circularBuffer;
    [SerializeField]
    private int sizeOfCircularBuffer;

    List<DataPoint> latestDatapointsList = new List<DataPoint>();

    
    

    DateTime startTime;

    public TMP_InputField ReadingFrequencyInputField;

    #endregion

    #region "Loop Variables"
    [Header("Loop Variables")]
    public int dataArraySize = 100000;
    private DataPoint[] dataArray;
    
    public int latestCheckedIteration = 0;
    public int currentIteration = 0;
    int iterations = 0;

    #endregion

    #region "Main Methods"
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        chart.EnsureChartComponent<Title>().text = "Real-Time Torque Data";
        chart.ClearData();
        //chart.AddSerie<Line>("Torque");

        // Optional: Customize appearance
        chart.GetSerie(0).symbol.show = false;

        serie = chart.GetSerie(0);
        serie.maxCache = serieMaxCache;
        serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin

        

        //LabJack
        InitializeValues();
        ConnectLabJack();
        dataArray = new DataPoint[dataArraySize];
        latestDatapointsList = new List<DataPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        timeCounter += Time.deltaTime;
        //LabJack
        if (timerIsRunning)
        {
            timeReading += Time.deltaTime;
            if (timeReading > maxTimeReadLoop)
                timerIsRunning = false;
        }

        //XCharts
        if (timerIsRunning && updatingChart)
        {
            Debug.Log("Trying to Update Chart");
            if (UpdateOneByOne)
            {
                UpdateChartLatestDatapoint();
                latestDatapointsList.Clear();
            }
            else
            {
                UpdateChartAllMissingDataPoints();
            }
        }

    }

    #endregion

    #region "Xcharts Main Methods

    public void UpdateChartLatestDatapoint()
    {
        AddTorqueDataPoint(latestDataPoint.AIN0);
    }

    public void UpdateChartAllMissingDataPoints()
    {
        // if (latestDatapointsList.Count != 0)
        // {
        //      Debug.Log("Adding List of Datapoints");
        // //     foreach (DataPoint dataPoint in latestDatapointsList)
        // //     {
        // //         //AddTorqueDataPoint(dataPoint);
        // //         //chart.RefreshChart();
        // //     }
        // //     chart.RefreshChart();
        // //     latestDatapointsList.Clear();
        // //     Debug.Log("List Added.");
        // }
        currentIteration = iterations;

        // for (int i = latestCheckedIteration; i < currentIteration; ++i)
        // {
        //     AddTorqueDataPoint(dataArray[i]);
        // }

        while (latestCheckedIteration < currentIteration)
        {
            AddTorqueDataPoint(dataArray[latestCheckedIteration]);
            ++latestCheckedIteration;
        }
        latestCheckedIteration = currentIteration;
    }

    #endregion

    #region "LabJack Main Methods

    public void StartRecording()
    {
        if (!isConnected)
        {
            Debug.LogError("Cannot start stream. LabJack is not connected.");
            //UpdateStatus("No connected LabJack.");
            return;
        }

        if (!isRunning)
        {
            Debug.Log("Starting stream...");
            isRunning = true;

            // Start the background thread for reading
            readThread = new Thread(ReadLoop);
            readThread.IsBackground = true;
            readThread.Start();

            //UpdateStatus("Streaming started...");

            //stopStreamButton.interactable = true;
            //recordButton.interactable = true;
            //startStreamButton.interactable = false;
        }
    }

    public void ReadLoop()
    {
        // Main loop: Read Every Interval

        Debug.Log("\nStarting read loop.");

        
        dataArray = new DataPoint[dataArraySize];

        /// StartInterval() Method:
        /// Sets up a reoccurring interval timer
        /// Interval is based on the host clock.
        /// Interval keeps reoccuring.
        /// Can be used with WaitForNextInterval(see inside while loop): 
        ///     wait/blocks/sleeps until next interval occurence
        LJM.StartInterval(intervalHandle, IntervalReadingInMicroseconds);

        /// 6. Choose which registers to read
        //Setup and call eReadNames to read AIN0, and FIO6 (T4) or
        //FIO2 (T7 and other devices).
        if (devType == LJM.CONSTANTS.dtT4)
        {
            aNames = new string[] { "AIN0", "FIO6" };
        }
        else
        {
            aNames = new string[] { "AIN0", "FIO2" };
        }
        aValues = new double[] { 0, 0 };
        numFrames = aNames.Length;

        // While Loop:
        startTime = DateTime.Now;

        iterations = 0;

        totalSkippedIntervals = 0;
        timerIsRunning = true;
        timeReading = 0;
        //while (!Console.KeyAvailable) //: Console.KeyAvailable: becomes true when the user has pressed a key that hasn’t been read yet
        //while (iterations < MaxIterations)
        while (timerIsRunning)
        {

            //Note: all the logs create performance issues, 
            // so it might be beneficial to comment it all for high frequency recordings

            // 7. Read the values
            LJM.eReadNames(handle, numFrames, aNames, aValues, ref errorAddress);

            // // 8. Log what was read
            // Debug.Log("eReadNames  :");
            // for (int i = 0; i < numFrames; i++)
            //     Debug.Log(" " + aNames[i] + " = " + aValues[i].ToString("F4") + ", ");

            // 8c. Fill the buffers with the new entry
            latestDataPoint.AIN0 = aValues[0];
            if (recordWithDateTime)
            {
                latestDataPoint.time = DateTime.Now;
            }

            latestDatapointsList.Add(latestDataPoint);

            dataArray[iterations] = latestDataPoint;
            //circularBuffer.Add(latestDataPoint);
            //bufferToSave[iterations] = aValues[0];

            // 9. Housekeeping for next iteration

            ++iterations;
            //Debug.Log($"End of Iteration #{iterations}.");

            // 10. Fixed‑rate timing
            //Wait for next 1 second interval
            LJM.WaitForNextInterval(intervalHandle, ref skippedIntervals);
            if (skippedIntervals > 0)
            {
                //Debug.Log("SkippedIntervals: " + skippedIntervals);
                totalSkippedIntervals += skippedIntervals;
            }



        }

        // 11. Loop ends
        Debug.Log("Read Loop ended");
        timerIsRunning = false;

        StopRecording();
        latestDatapointsList.Clear();
    }

    

    #endregion

    #region "XCharts Helper Methods"

    public void AddTorqueDataPoint(float torque)
    {
        // Add new data
        serie.AddData(timeCounter, torque);

        // // Keep data length manageable
        // if (serie.dataCount > maxDataPoints)
        // {
        //     serie.RemoveData(0);
        // }

        chart.RefreshChart(); // Force refresh
    }

    public void AddTorqueDataPoint(double torque)
    {
        // Add new data
        serie.AddData(timeCounter, torque);

        // Keep data length manageable
        if (serie.dataCount > maxDataPoints)
        {
            serie.RemoveData(0);
        }
        chart.RefreshChart();
    }
    public void AddTorqueDataPoint(DataPoint dataPoint)
    {
        // Later, for each new data point:
        elapsedMs = (dataPoint.time - startTime).TotalMilliseconds;
        serie.AddData(elapsedMs, dataPoint.AIN0);

        // if (serie.dataCount > maxDataPoints)
        // {
        //     serie.RemoveData(0);
        // }
    }

    public void ClearGraph()
    {
        chart.ClearData();
    }

    public static double ToUnixMilliseconds(DateTime time)
    {
        Debug.Log("Conversion to UnixMilliseconds");
        return (time.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds;
    }

    public void OnChangeUpdateOneByOne()
    {
        UpdateOneByOne = UpdateOneByOneToggle.isOn;
    }

    public void OnChangeUpdatingChart()
    {
        updatingChart = updatingChartInput.isOn;
    }

    #endregion

    #region "LabJack Helper Methods"

    public void ConfigLabJack()
    {
        //LabJack T7 and T8 configuration

        //Settling and negative channel do not apply to the T8
        if (devType == LJM.CONSTANTS.dtT7)
        {
            aNames = new string[] { "AIN0_NEGATIVE_CH",
                                    "AIN0_SETTLING_US"};

            aValues = new double[] { 199, 0 };
            // By definition numFrames = size of aNames
            numFrames = aNames.Length;

            LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errorAddress);
        }

        aNames = new string[] { "AIN0_RANGE",
                                    "AIN0_RESOLUTION_INDEX"};
        aValues = new double[] { 10,   //  Range ±10V: full voltage swing allowed.
                                        0 }; //  Resolution index 0: lowest (fastest) resolution.
        numFrames = aNames.Length;
        // Same memory allocation as above
        LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errorAddress);
    }

    public void ConnectLabJack()
    {
        //Open first found LabJack
        LJM.OpenS("ANY", "ANY", "ANY", ref handle);

        // Get and Display Device Info
        LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
        LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
        // Converts numeric IP to a readable string.
        LJM.NumberToIP(ipAddr, ref ipAddrStr);

        Debug.Log("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");
        Debug.Log("  Serial number: " + serNum + ", IP address: " + ipAddrStr + ", Port: " + port + ",");
        Debug.Log("  Max bytes per MB: " + maxBytesPerMB);
        isConnected = true;

    }

    public void DisconnectLabJack()
    {
        //LJM.CleanInterval(intervalHandle);
        LJM.CloseAll();
        isConnected = false;
        Debug.Log("Done");
    }

    public void InitializeValues()
    {
        handle = 0;
        devType = 0; // Device type (T4, T7, T8)
        conType = 0; // USB, Ethernet, WiFi
        serNum = 0;  // Serial number
        ipAddr = 0;  // IP address (numeric form)
        ipAddrStr = "";
        port = 0;
        maxBytesPerMB = 0;

        errorAddress = -1;

        intervalHandle = 1; // For timed reading every second
        numFrames = 0;

        sizeOfCircularBuffer = 10;
        circularBuffer = new CircularBuffer<DataPoint>(sizeOfCircularBuffer);

        bufferToSave = new double[sizeOfBufferToSave];

        EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
        EditorApplication.pauseStateChanged += HandleOnPlayModeChanged;
        Debug.Log("Values Initialized");
    }

    public void StopRecording()
    {
        if (isRunning)
        {
            Debug.Log("Stopping stream...");
            isRunning = false;

            // Wait for the thread to terminate
            if (readThread != null && readThread.IsAlive)
            {
                readThread.Join();
            }

            //UpdateStatus("Streaming stopped.");

            // // Stop recording if it was active
            // if (isRecording)
            // {
            //     StopRecording();
            // }

            //stopStreamButton.interactable = false;
            //recordButton.interactable = false;
            //startStreamButton.interactable = true;
        }
    }

    public void UpdateReadMaxTime()
    {
        if (maxTimeUserInput.text != "")
            maxTimeReadLoop = double.Parse(maxTimeUserInput.text);
    }

    public void UpdateReadingInterval()
    {
        if (ReadingIntervalInput.text != "")
        {
            IntervalReadingInMicroseconds = int.Parse(ReadingIntervalInput.text);
        }
    }

    public void UpdateRecordingMode()
    {
        recordWithDateTime = recordWithDateTimeInput.isOn;
    }


    private void HandleOnPlayModeChanged(PlayModeStateChange state)
    {
        // This method is run whenever the playmode state is changed.

        if (EditorApplication.isPaused)
        {
            // do stuff when the editor is paused.
            StopRecording();
        }
        if (!EditorApplication.isUpdating)
        {
            StopRecording();
        }

        if (!EditorApplication.isPlaying)
        {
            StopRecording();
        }
    }
    private void HandleOnPlayModeChanged(PauseState state)
    {
        // This method is run whenever the playmode state is changed.

        if (EditorApplication.isPaused)
        {
            // do stuff when the editor is paused.
            StopRecording();
        }
        if (!EditorApplication.isUpdating)
        {
            StopRecording();
        }

        if (!EditorApplication.isPlaying)
        {
            StopRecording();
        }
    }

    public void UpdateReadingFreq()
    {
        if (ReadingFrequencyInputField.text != "" && ReadingFrequencyInputField.text != "0")
            IntervalReadingInMicroseconds = (int)Math.Round(1000000 / double.Parse(ReadingFrequencyInputField.text));
    }

    #endregion
}
