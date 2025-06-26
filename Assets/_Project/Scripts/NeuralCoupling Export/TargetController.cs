using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    public bool stop;
    public GameObject imuSensor1;
    public GameObject imuSensor2;
    public GameObject imuSensor3;
    public GameObject imuSensor4;
    public GameObject calibrationCircle;
    public GameObject calibrationValue;
    public float[] emgData;
    public Quaternion[] quaternionData;
    public float rotLeft;
    public float rotRight;
    public float deviceAngle;
    public GameObject userRoot;
    public WaveplusDaqScript wavePlusInstance;
    public GameObject serialPortObject;

    private bool stimulationOK = true;

    private float maxAngle = -Mathf.Infinity;
    private float minAngle = Mathf.Infinity;
    private bool isCalibrating = false;

    private Color stdCalibColor;

    private void Start()
    {
        // Initialize variables
        emgData = new float[4];
        for (int ii = 0; ii < emgData.Length; ++ii)
        {
            emgData[ii] = 0f;
        }
        quaternionData = new Quaternion[4];
        for (int kk = 0; kk < quaternionData.Length; ++kk)
        {
            quaternionData[kk] = Quaternion.identity;
        }

        stdCalibColor = calibrationCircle.GetComponentInChildren<Image>().color;
    }

    private void Update()
    {
        imuSensor1.transform.rotation = wavePlusInstance.imuSensorValues[0];
        imuSensor2.transform.rotation = wavePlusInstance.imuSensorValues[1];
        imuSensor3.transform.rotation = wavePlusInstance.imuSensorValues[2];
        imuSensor4.transform.rotation = wavePlusInstance.imuSensorValues[3];

        rotLeft = -(UnwrapAngle(imuSensor1.transform.eulerAngles.x) - UnwrapAngle(imuSensor2.transform.eulerAngles.x));
        rotRight = -(UnwrapAngle(imuSensor3.transform.eulerAngles.x) - UnwrapAngle(imuSensor4.transform.eulerAngles.x));

        deviceAngle = rotLeft - rotRight;

        // Set calibration angles
        if(isCalibrating)
        { 
            if(deviceAngle < minAngle)
            {
                minAngle = deviceAngle;
            }

            if(deviceAngle > maxAngle)
            {
                maxAngle = deviceAngle;
            }
        }

        // If user is allowed to move into position
        if (UIManagerScript.isUserAllowedToMove)
        {
            // Map current device angle to a range between 0 and 4
            float position = 4 * (deviceAngle - minAngle) / (maxAngle - minAngle);
            // If user position is 0 or not defined
            if (position == 0 || float.IsNaN(position))
            {
                userRoot.transform.localScale = new Vector3(userRoot.transform.localScale.x, 0.000001f, userRoot.transform.localScale.z);
            }
            else
            {
                // Scale user position to correspond to angle
                userRoot.transform.localScale = new Vector3(userRoot.transform.localScale.x, position, userRoot.transform.localScale.z);
            }
            
            //Debug.Log("User Root: " + userRoot.transform.localScale.y.ToString());
            //Debug.Log("Threshold Value: " + UIManagerScript.stimThreshold / 100f * 4);
            //Debug.Log("Stim Trial: " + UIManagerScript.isStimTrial);
            // If trial is running
            if (UIManagerScript.isGameRunning)
            {
                // Check if user position is higher than stimulation threshold
                if (userRoot.transform.localScale.y > (UIManagerScript.stimThreshold / 100f * 4))
                {
                    // If the current trial is a stimulation trial
                    if (UIManagerScript.isStimTrial)
                    {
                        // Stimulate user
                        try
                        {
                            // Trigger stimulation train on Teensy
                            serialPortObject.GetComponent<SerialPort2Teensy>().triggerStimulation();
                            // Change flag that stimulation has been given
                            UIManagerScript.hasStimBeenGiven = true;
                            UIManagerScript.isStimTrial = false;
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.ToString());
                        }
                    }
                    else if (UIManagerScript.isDummyTrial) //we have a dummy trial
                    {
                        // Trigger dummy recording
                        UIManagerScript.shouldDummyBeRecorded = true;
                        UIManagerScript.hasDummyBeenGiven = true;
                        UIManagerScript.isDummyTrial = false;
                    }
                }
            }
        }
    }

    private static float UnwrapAngle(float angle)
    {
        if (angle >= 180)
            return angle -= 360;
        else
            return angle;
    }

    public void CalibrateAngle(Button button)
    {
        if (isCalibrating == false)
        {
            isCalibrating = true;
            // Visualize calibration
            calibrationCircle.GetComponentInChildren<Image>().color = Color.red;
        }
        else
        {
            isCalibrating = false;
            calibrationCircle.GetComponentInChildren<Image>().color = stdCalibColor;
            calibrationValue.GetComponentInChildren<Text>().text = (maxAngle - minAngle).ToString();
        }
    }

    public void ResetCalibratedAngle()
    {
        maxAngle = -Mathf.Infinity;
        minAngle = Mathf.Infinity;
        calibrationValue.GetComponentInChildren<Text>().text = "-";
    }
}