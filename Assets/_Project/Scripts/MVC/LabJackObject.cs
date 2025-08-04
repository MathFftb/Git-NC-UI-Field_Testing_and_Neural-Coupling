using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Threading;
using LabJack;
using TMPro;
using UnityEngine.Events;

[Serializable]
public class LabJackObject // Note: All configuration for a model T7, not T4
{
    #region "State Variables"
    [Header("State Variables")]
    public bool isConnected = false;
    public bool isRunning = false; // The LabJack is running if it is rading or writing something

    #endregion

    #region "Configuration Variables"
    [Header("Configuration Variables")]

    // These string[] are not used for now as Unity cannot modify arrays in the inspector
    // Could be uncommented to use List<string> instead
    // For now the equivalent is hardcoded
    // public string[] registersToReadNames = new string[] { "AIN0", "FIO2" };
    // public string[] negativeAndSettlingChannelsNames = new string[] { "AIN0_NEGATIVE_CH", "AIN0_SETTLING_US"};
    // public string[] otherConfigNames;



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

    #endregion

    #region "Data Variables"
    [Serializable]
    public struct LabJackDataPoint : IComparable<LabJackDataPoint>
    {
        public double AIN0;
        public DateTime time;

        public LabJackDataPoint(double _ANI0, DateTime _time)
        {
            AIN0 = _ANI0;
            time = _time;
        }

        public void SetToZero()
        {
            AIN0 = 0;
            time = default(DateTime);
        }

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

        public int CompareTo(LabJackDataPoint other)
        {
            return this.AIN0.CompareTo(other.AIN0);
        }
    }

    public int dataArrayMaxSize = 100000;
    public LabJackDataPoint[] dataArray;
    [SerializeField]
    private LabJackDataPoint latestDataPoint;

    #endregion

    #region "Read Loop Variables"
    [Header("Read Loop Variables")]

    public int intervalReadingInMicroseconds = 1000000; // Note: Intervals in LabJack are given in Microseconds
    public double readingFreqHz { get => 1000000 / intervalReadingInMicroseconds; } // Convenience Variable converting IntervalReading from Ms to Hz
    public double maxTimeReadLoopSec = 10; // Note: Time.DeltaTime in Unity is given in Seconds
    public bool timerIsRunning = false;
    public int latestIteration = 0;
    public double timeReadingSec = 0;
    public int skippedIntervals = 0;
    public int totalSkippedIntervals = 0;

    // Unity Events to communicate the start and end of a reading
    // Important: These cannot be called from the ReadLoop separate Thread
    // If necessary, it would require a thread dispatcher using enqueue
    public UnityEvent OnLabJackReadingStart;
    public UnityEvent OnLabJackReadingEnd;


    // Additional thread to run the read loop separately
    private Thread readThread;

    // Variables managing time counting
    public DateTime startLoopTime;



    int numFrames = 0;
    string[] aNames; // array of strings there to be filled and passed for command in this code
    double[] aValues; // array of doubles there to be filled and passed for command in this code



    #endregion

    #region "Main Methods"

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeAllValues();
    }

    // Update is called once per frame
    // (!)Note: because LabJackObject is an object and not a monobehaviour, 
    // this function will be called in the Update method of the scripts using the labjack
    public void Update()
    {
        //LabJack
        if (timerIsRunning)
        {
            timeReadingSec += Time.deltaTime;
            if (timeReadingSec > maxTimeReadLoopSec)
                timerIsRunning = false;
        }
    }

    #endregion

    #region "Configuration Methods"
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

    #endregion

    #region "Initialisation Methods"

    public void InitializeAllValues()
    {
        InitializeConfigurationVariables();

        InitializeDataArray();

        InitializePlaymodeEvents();
    }

    public void InitializeConfigurationVariables()
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
    }


    public void InitializeDataArray()
    {
        dataArray = new LabJackDataPoint[dataArrayMaxSize];
    }

    public void InitializePlaymodeEvents()
    {
        EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
        EditorApplication.pauseStateChanged += HandleOnPlayModeChanged;
        Debug.Log("Values Initialized");
    }


    #endregion

    #region "Start Loops Methods"
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

            // Unity Event
            OnLabJackReadingStart.Invoke();
        }
    }


    #endregion

    #region "Looping Methods"
    public void ReadLoop()
    {
        // Main loop: Read Every Interval
        Debug.Log("Starting read loop.");

        // 1. Initialize array to be filled
        dataArray = new LabJackDataPoint[dataArrayMaxSize];

        /// 2. StartInterval() Method:
        /// Sets up a reoccurring interval timer
        /// Interval is based on the host clock.
        /// Interval keeps reoccuring.
        /// Used in tandem with WaitForNextInterval(see inside while loop): 
        ///     waits/blocks/sleeps until next interval occurence
        LJM.StartInterval(intervalHandle, intervalReadingInMicroseconds);

        /// 3. Choose which registers to read
        // Setup and call eReadNames to read AIN0, and FIO2 (T7 and other devices) or FIO6 (T4).
        if (devType != LJM.CONSTANTS.dtT4)
        {
            aNames = new string[] { "AIN0", "FIO2" };
        }
        else
        {
            aNames = new string[] { "AIN0", "FIO6" };
            Debug.LogError("Possible error: LabJack Type was supposed to be T7 for this script");
        }
        aValues = new double[] { 0, 0 };
        // By definition numFrames = size of aNames
        numFrames = aNames.Length;

        // 4. Set all zero variables before starting loop
        startLoopTime = DateTime.Now;
        timerIsRunning = true;
        latestIteration = 0;
        timeReadingSec = 0;
        // Note: timeReading is updated in Update() as the thread of ReadLoop does not have access to DeltaTime
        // Could also be updated in the while loop using system time and comparing with startTime
        // Note: timerIsRunning is updated in Update() for the same reasons
        totalSkippedIntervals = 0;

        // 5. While Loop Condition
        //while (!Console.KeyAvailable) //Note: Console.KeyAvailable becomes true when the user has pressed a key that hasn’t been read yet
        //while (iterations < MaxIterations)
        while (timerIsRunning && isRunning) // While loop runs until the timer runs out or a command sets isRunning to false
        {

            //Note: all the logs create performance issues, 
            // so it might be beneficial to comment it all for high frequency recordings

            // 6. Read the values
            LJM.eReadNames(handle, numFrames, aNames, aValues, ref errorAddress);

            // 7. Load what was read
            latestDataPoint.AIN0 = aValues[0];

            // 7.b Load DataPoint with timepoint
            latestDataPoint.time = DateTime.Now;

            // 7c. Fill the buffers with the new entry
            dataArray[latestIteration] = latestDataPoint;

            // 8. Housekeeping for next iteration
            ++latestIteration;

            // 9. Check for missed intervals
            if (skippedIntervals > 0)
            {
                //Debug.Log("SkippedIntervals: " + skippedIntervals);
                totalSkippedIntervals += skippedIntervals;
            }

            // 10. Fixed‑rate timing
            // Wait for next interval event in skippedIntervals microseconds
            LJM.WaitForNextInterval(intervalHandle, ref skippedIntervals);

        }

        // 11. Loop ends
        Debug.Log("Read Loop ended");
        timerIsRunning = false;

        StopRecording();
    }

    #endregion

    #region "Stopping Loops Methods" 
    public void DisconnectLabJack()
    {
        //LJM.CleanInterval(intervalHandle);
        LJM.CloseAll();
        isConnected = false;
        Debug.Log("LabJack Disconnected");
    }

    public void StopRecording()
    {
        if (isRunning)
        {
            Debug.Log("Stopping stream...");
            isRunning = false;

            // Unity event and Thread managing
            Debug.Log($"Stopping stream on thread: {Thread.CurrentThread.ManagedThreadId}");
            // Schedule safe callback on main thread
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log($"Now on main thread: {Thread.CurrentThread.ManagedThreadId}");
                OnLabJackReadingEnd.Invoke(); // Safe here
            });

            // Stopping the Thread
            // Wait for the thread to terminate
            if (readThread != null && readThread.IsAlive)
            {
                readThread.Join();
            }
          
        }
    }
    #endregion

    #region "Cleaning Methods"
    public void CleanLabJackArray(int lastEntry)
    {
        lastEntry = Math.Min(lastEntry, dataArray.Length);
        for (int i = 0; i < lastEntry; ++i)
            {
                dataArray[i].SetToZero();
            }
    }

    public void CleanLabJackArray()
    {
        CleanLabJackArray(dataArray.Length);
    }
    #endregion

    #region 
    #endregion

    #region "PlayMode Changed Methods
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
    #endregion
}