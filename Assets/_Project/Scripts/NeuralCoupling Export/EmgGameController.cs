using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class EmgGameController : MonoBehaviour
{   
    [SerializeField]
    private List<float> calibList;
    public float calibrationFactor;

    private SlidingAverage ringBuffer;
    public int ringBufferSize;
    private bool hasBufferSizeChanged;

    public static float emgUserInput;
    public static float sensorData;
    public bool isRectified;
    public bool isSmoothed;

    public Text calibrationValueLabel;
    public GameObject progressBarObject;
    private CircleFillHandler progressBarHandler;

    private bool isCoroutineRunning;


    // Start is called before the first frame update
    void Start()
    {
        // Create new empty ring buffer for smoothing data
        ringBuffer = new SlidingAverage(ringBufferSize, 0f);
        // Create new empty calibration list
        calibList = new List<float>();

        // Set calibration coroutine flag
        isCoroutineRunning = false;
        // Set initial calibrationFactor
        calibrationFactor = 1f;

        hasBufferSizeChanged = false;
        sensorData = 0;
        emgUserInput = 0;

        // Get reference to calibration progress bar
        progressBarHandler = progressBarObject.GetComponent<CircleFillHandler>();
        progressBarHandler.fillValue = 0f;
    }

    private void OnValidate()
    {
        hasBufferSizeChanged = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Check that buffer size has not changed
        if (hasBufferSizeChanged)
        {
            ringBuffer = new SlidingAverage(ringBufferSize, 0f);
            hasBufferSizeChanged = false;
            Debug.Log("Buffer size changed.");
        }

        //Debug.Log("Sensor Data: " + sensorData.ToString());
        sensorData = 0f;
        //sensorData = UDPClient.emgData[UIManagerScript.sensorIdx - 1];
        
        // If EMG data should be rectified
        if (isRectified)
        {
            // Get rectified emg
            sensorData = Mathf.Abs(sensorData);
        }

        // If EMG data should be smoothed
        if (isSmoothed)
        {
            // Add new EMG value to ring buffer
            ringBuffer.pushValue(sensorData);
            // Filter EMG based on sliding window
            sensorData = ringBuffer.getSmoothedValue();
        }

        emgUserInput = Mathf.Clamp01(sensorData / calibrationFactor);
    }

    public void CalibrateEMG(Button button)
    {
        // Check that no calibration is already in process
        if(!isCoroutineRunning)
        {
            // Prevent new calibrations from starting
            button.interactable = false;
            // Start calibration routine
            IEnumerator coroutine = GetEMGCalibration(button);
            StartCoroutine(coroutine);
        }
    }

    IEnumerator GetEMGCalibration(Button button)
    {
        // Set flag that calibration is in progress
        isCoroutineRunning = true;
        // Set calibration amount of seconds
        const float waitTime = 3f;
        // Initialize timer
        float timer = 0f;
        // Clear list of calibration values
        calibList.Clear();

        // Run through timer
        while (timer < waitTime)
        {    
            // Add sensor data to calibration list
            calibList.Add(sensorData);
            // Increase timer
            timer += Time.unscaledDeltaTime;

            // Update progress bar
            progressBarHandler.fillValue = Mathf.Clamp(timer * 100f / 3f, 0f, 100f);

            //Wait for a frame
            yield return null;
        }

        // Finial progress bar
        progressBarHandler.fillValue = 100f;

        // Get mean sensor value from calibration data
        calibrationFactor = calibList.Sum() / calibList.Count;

        // Update calibration value label
        calibrationValueLabel.text = calibrationFactor.ToString("F2");

        // Enable new calibration by resetting parameters
        isCoroutineRunning = false;
        button.interactable = true;

        // Reset progress bar
        progressBarHandler.fillValue = 0f;
    }
}
