using UnityEngine;


using System;
using System.IO;
using System.Threading;
using LabJack; // LabJack LJM library
using UnityEngine.UI;
 
public class LabJackExampleUse : MonoBehaviour
{
    /*private int deviceHandle = 0;
    private double ainValue = 0.0;
    private bool isRunning = false;
    private bool isRecording = false;
    private bool isConnected = false;
    private StreamWriter logWriter;
 
    private Thread readThread;
 
    [Header("UI Elements")]
    public Button startStreamButton;
    public Button stopStreamButton;
    public Button recordButton;
    public Text statusText;
 
    private string logFilePath;
 
    void Start()
    {
        Debug.Log("Starting LabJack Controller...");
 
        // Attach button listeners
        startStreamButton.onClick.AddListener(StartStream);
        stopStreamButton.onClick.AddListener(StopStream);
        recordButton.onClick.AddListener(StartRecording);
 
        // Initially disable buttons
        DisableAllButtons();
 
        // Start the coroutine to poll for connection
        StartCoroutine(PollForConnection());
    }
 
    void Update()
    {
        // Display the AIN0 value in the Unity main thread
        if (isRunning)
        {
            Debug.Log($"AIN0 Value: {ainValue}");
 
            // Example: Adjust the object's scale based on the AIN0 value
            transform.localScale = Vector3.one * Mathf.Clamp((float)ainValue, 0.1f, 5.0f);
        }
    }
 
    private System.Collections.IEnumerator PollForConnection()
    {
        while (true)
        {
            try
            {
                if (!isConnected)
                {
                    // Attempt to detect and connect to a LabJack device
                    int numDevices = 0;
                    LJM.DeviceInfo[] devices = LJM.ListAll(LJM.Constants.dtANY, LJM.Constants.ctUSB, out numDevices);
 
                    if (numDevices > 0)
                    {
                        Debug.Log($"Detected {numDevices} LabJack device(s). Connecting...");
                        deviceHandle = LJM.OpenS("ANY", "ANY", "ANY");
                        Debug.Log($"Connected to LabJack with handle: {deviceHandle}");
 
                        // Configure the AIN0 channel
                        LJM.eWriteName(deviceHandle, "AIN0_RANGE", 10.0);
                        LJM.eWriteName(deviceHandle, "AIN0_RESOLUTION_INDEX", 0);
 
                        isConnected = true;
                        EnableStartButton();
                        UpdateStatus("LabJack connected. Ready to stream.");
                    }
                    else
                    {
                        DisableAllButtons();
                        isConnected = false;
                    }
                }
                else
                {
                    // If connected, test if the LabJack is still reachable
                    try
                    {
                        LJM.eReadName(deviceHandle, "AIN0");
                    }
                    catch
                    {
                        Debug.LogWarning("LabJack disconnected.");
                        Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection Polling Error: {ex.Message}");
            }
 
            // Wait for 2 seconds before polling again
            yield return new WaitForSeconds(2);
        }
    }
 
    private void StartStream()
    {
        if (!isConnected || deviceHandle == 0)
        {
            Debug.LogError("Cannot start stream. LabJack is not connected.");
            UpdateStatus("No connected LabJack.");
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
 
            UpdateStatus("Streaming started...");
 
            stopStreamButton.interactable = true;
            recordButton.interactable = true;
            startStreamButton.interactable = false;
        }
    }
 
    private void StopStream()
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
 
            UpdateStatus("Streaming stopped.");
 
            // Stop recording if it was active
            if (isRecording)
            {
                StopRecording();
            }
 
            stopStreamButton.interactable = false;
            recordButton.interactable = false;
            startStreamButton.interactable = true;
        }
    }
 
    private void StartRecording()
    {
        if (!isRunning)
        {
            Debug.LogError("Cannot start recording unless the stream is running.");
            UpdateStatus("Start the stream before recording.");
            return;
        }
 
        if (!isRecording)
        {
            Debug.Log("Starting recording...");
            isRecording = true;
 
            // Open a file to log data
            logFilePath = Path.Combine(Application.persistentDataPath, $"LabJackLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            logWriter = new StreamWriter(logFilePath);
            logWriter.WriteLine("Timestamp,AIN0");
 
            UpdateStatus($"Recording started. Logging to: {logFilePath}");
        }
    }
 
    private void StopRecording()
    {
        if (isRecording)
        {
            Debug.Log("Stopping recording...");
            isRecording = false;
 
            if (logWriter != null)
            {
                logWriter.Close();
                logWriter = null;
            }
 
            UpdateStatus("Recording stopped.");
        }
    }
 
    private void ReadLoop()
    {
        try
        {
            // Set the interval to 100 ms (10 Hz)
            LJM.StartInterval(100000);
 
            while (isRunning)
            {
                try
                {
                    // Read the analog input value from AIN0
                    ainValue = LJM.eReadName(deviceHandle, "AIN0");
 
                    // Log the value if recording
                    if (isRecording && logWriter != null)
                    {
                        logWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff},{ainValue}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading AIN0: {ex.Message}");
                    StopStream(); // Stop the stream if there's a read error
                }
 
                // Wait for the interval
                LJM.WaitForNextInterval();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ReadLoop Error: {ex.Message}");
        }
        finally
        {
            Debug.Log("Read loop terminated.");
        }
    }
 
    private void Disconnect()
    {
        if (deviceHandle != 0)
        {
            LJM.Close(deviceHandle);
            deviceHandle = 0;
        }
 
        isConnected = false;
        isRunning = false;
 
        DisableAllButtons();
        UpdateStatus("LabJack disconnected.");
    }
 
    private void DisableAllButtons()
    {
        startStreamButton.interactable = false;
        stopStreamButton.interactable = false;
        recordButton.interactable = false;
    }
 
    private void EnableStartButton()
    {
        startStreamButton.interactable = true;
        stopStreamButton.interactable = false;
        recordButton.interactable = false;
    }
 
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
 
    private void OnDestroy()
    {
        isRunning = false;
        isConnected = false;
 
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
 
        if (deviceHandle != 0)
        {
            LJM.Close(deviceHandle);
        }
 
        if (isRecording)
        {
            StopRecording();
        }
    }*/
}
