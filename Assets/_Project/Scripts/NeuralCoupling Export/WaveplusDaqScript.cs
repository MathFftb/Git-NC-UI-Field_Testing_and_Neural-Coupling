using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Waveplus.DaqSys;
using Waveplus.DaqSysInterface;
using WaveplusLab.Shared.Definitions;

public class WaveplusDaqScript : MonoBehaviour
{
    internal IDaqSystem _daqSystem;
    [SerializeField]
    private bool _isFirstIdleState;
    [SerializeField]
    private int _acquiredSamplesPerChannel;
    [SerializeField]
    private int _importedTrials;
    [SerializeField]
    public int _installedSensor;
    [SerializeField]
    public bool _isAcquiring = false;
    public Button recButton;
    public UIManagerScript uiScript;

    private const int nEMGSensors = 4;
    public float[] emgSensorValues = new float[nEMGSensors];
    private const int nIMUSensors = 4;
    public Quaternion[] imuSensorValues = new Quaternion[nIMUSensors];

    private int bufferCounter = 0;
    private Quaternion[] imu1 = new Quaternion[1200000];
    private Quaternion[] imu2 = new Quaternion[1200000];
    private Quaternion[] imu3 = new Quaternion[1200000];
    private Quaternion[] imu4 = new Quaternion[1200000];
    private float[] emg1 = new float[1200000];
    private float[] emg2 = new float[1200000];
    private float[] emg3 = new float[1200000];
    private float[] emg4 = new float[1200000];
    private int[] trigger = new int[1200000];
    private int[] dummies = new int[1200000];



    // Start is called before the first frame update
    void Start()
    {
        _isFirstIdleState = true;
        _isAcquiring = false;
        //recButton.interactable = false;
        _installedSensor = 0;

        // Create sensor status controls
        /*_sensorNumberLabels = new Label[DeviceConstant.MAX_SENSORS_NUM];
        _sensorBatteryLevelBars = new ProgressBar[DeviceConstant.MAX_SENSORS_NUM];

        // Create sensor battery level status controls
        const int startY = 5;
        const int deltaY = 20;
        for (var i = 0; i < DeviceConstant.MAX_SENSORS_NUM; i++)
        {
            SensorStatePanel.Controls.Add(_sensorNumberLabels[i] = new Label
            {
                Visible = true,
                AutoSize = false,
                Size = new Size(19, 13),
                Location = new Point(6, startY + i * deltaY),
                Text = (i + 1).ToString(),
                Enabled = true
            });
            SensorStatePanel.Controls.Add(_sensorBatteryLevelBars[i] = new ProgressBar
            {
                Visible = true,
                AutoSize = false,
                Size = new Size(34, 10),
                Location = new Point(31, startY + 2 + i * deltaY),
                Text = (i + 1).ToString(),
                Maximum = 3,
                Step = 1,
                ForeColor = Color.LimeGreen,
                Value = 0,
                Enabled = true
            });
        }

        // Set DataAvailableEvent period
        comboBoxDataAvailableEventPeriod.Items.AddRange((Enum.GetNames(typeof(DataAvailableEventPeriod))));
        // DataAvailableEvent period
        comboBoxDataAvailableEventPeriod.SelectedIndex = comboBoxDataAvailableEventPeriod.FindStringExact(DataAvailableEventPeriod.ms_100.ToString());
        // Add RF channel items
        ChangeMasterRFChannelNewComboBox.Items.AddRange((Enum.GetNames(typeof(RFChannel))));
        // Select RF channel
        ChangeMasterRFChannelNewComboBox.SelectedIndex = ChangeMasterRFChannelNewComboBox.FindStringExact(RFChannel.RFChannel_0.ToString());*/

        try
        {
            // Create _daqSystem object and assign the event handlers
            _daqSystem = new DaqSystem();
            _daqSystem.StateChanged += Device_StateChanged;
            _daqSystem.DataAvailable += Capture_DataAvailable;
            _daqSystem.SensorMemoryDataAvailable += SensorMemory_DataAvailable;

            // Configure IMU data acquisition
            CaptureConfiguration imuConfiguration = new CaptureConfiguration(); //initialize new configuration
            imuConfiguration.IMU_AcqType = ImuAcqType.Mixed6xData_142Hz; //6 axis fused data (quaternions) acquisition at 142 Hz
            _daqSystem.ConfigureCapture(imuConfiguration);  //apply new configuration
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
        finally
        {
            // Show device state
            ShowDeviceState(_daqSystem.State);
            DisplayErrorOccurred(_daqSystem.InitialError);
        }
    }

    private void OnApplicationQuit()
    {
        if(_daqSystem != null)
        {
            // Remove the event handlers from _daqSystem object and dispose it
            _daqSystem.StateChanged -= Device_StateChanged;
            _daqSystem.DataAvailable -= Capture_DataAvailable;
            _daqSystem.SensorMemoryDataAvailable -= SensorMemory_DataAvailable;
            _daqSystem.Dispose();
        }
    }

    void Device_StateChanged(object sender, DeviceStateChangedEventArgs e)
    {
        /*if (InvokeRequired)
        {
            MethodInvoker invoke = () => Device_StateChanged(sender, e);
            Invoke(invoke);
            return;
        }*/
        // Show the new device state
        ShowDeviceState(e.State);
    }

    private string ShowDeviceState(DeviceState newState)
    {
        // Update the GUI state according to the new device state
        switch (newState)
        {
            case DeviceState.Idle:
                //Capture controls
                //StartCaptureButton.Enabled = true;
                //StopCaptureButton.Enabled = false;
                //CaptureGroupBox.Enabled = true;
                //GenerateStartTriggerButton.Enabled = false;
                //GenerateStopTriggerButton.Enabled = false;
                //CaptureGroupBox.Enabled = true;
                //Capture and Sensors configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = true;
                //Device controls
                //DeviceGroupBox.Enabled = true;
                if (_isFirstIdleState)
                {
                    // Every time the device is connected 
                    getInstalledSensors();
                    getInstalledFootSwSensors();
                    getHardwareVersion();
                    getFirmwareVersion();
                    getSoftwareVersion();
                    getMasterDeviceRFChannel();
                    // Remote recording enable button
                    //MemoryModeEnableCheckBox.Checked = false;
                    _isFirstIdleState = false;
                }
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = true;
                //Importing from memory controls
                //MemoryStartImportingButton.Enabled = true;
                //MemoryStopImportingButton.Enabled = false;
                //GetMemoryStatusButton.Enabled = true;
                //MemoryClearButton.Enabled = true;
                //SensorMemoryGroupBox.Enabled = true;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = true;
                break;
            case DeviceState.Capturing:
                //Capture controls
                //StartCaptureButton.Enabled = false;
                //StopCaptureButton.Enabled = true;
                //CaptureGroupBox.Enabled = true;
                //GenerateStartTriggerButton.Enabled = true;
                //GenerateStopTriggerButton.Enabled = true;
                //CaptureGroupBox.Enabled = true;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = false;
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Sensor memory controls
                //SensorMemoryGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;

            case DeviceState.ReadingSensorMemory:
                //Importing from memory controls
                //MemoryStartImportingButton.Enabled = false;
                //MemoryStopImportingButton.Enabled = true;
                //GetMemoryStatusButton.Enabled = false;
                //MemoryClearButton.Enabled = false;
                //SensorMemoryGroupBox.Enabled = true;
                // Capture controls
                //CaptureGroupBox.Enabled = false;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = false;
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Capture controls
                //CaptureGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;

            case DeviceState.NotConnected:
                //Capture controls
                //CaptureGroupBox.Enabled = false;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = false;
                _isFirstIdleState = true;
                _installedSensor = 0;
                //Reset the controls
                //FwVersionTextBox.Text = "";
                //HwVersionTextBox.Text = "";
                //ErrorMessageTextBox.Text = "";
                //InstalledSensorsTextBox.Text = "";
                //InstalledFootSwSensorsTextBox.Text = "";
                //ChangeMasterRFChannelCurrentTextBox.Text = "";
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Sensor memory controls
                //SensorMemoryGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;
            case DeviceState.CommunicationError:
                //Capture controls
                //CaptureGroupBox.Enabled = false;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = false;
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Sensor memory controls
                //SensorMemoryGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;
            case DeviceState.InitializingError:
                //Capture controls
                //CaptureGroupBox.Enabled = false;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = true;
                //Get hardware, firmware and software versions
                getHardwareVersion();
                getFirmwareVersion();
                getSoftwareVersion();
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Sensor memory controls
                //SensorMemoryGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;
            case DeviceState.UpdatingFirmware:
                //Capture controls
                //CaptureGroupBox.Enabled = false;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = false;
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Sensor memory controls
                //SensorMemoryGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;
            case DeviceState.Initializing:
                //Capture controls
                //CaptureGroupBox.Enabled = false;
                //Capture configuration controls
                //CaptionAndSensorsConfigurationGroupBox.Enabled = false;
                //Device controls
                //DeviceGroupBox.Enabled = false;
                // Change RF channel controls
                //ChangeMasterRFChannelGroupBox.Enabled = false;
                // Sensor memory controls
                //SensorMemoryGroupBox.Enabled = false;
                // Remote recording group
                //RemoteRecordingGroupBox.Enabled = false;
                break;
        }
        Debug.Log("Device status: " + newState.ToString());
        return newState.ToString();
    }

    private void DisplayErrorOccurred(DeviceError error)
    {
        // Display device error code
        //ErrorMessageTextBox.Text = error.ToString();
        Debug.Log("Error: " + error.ToString());
    }

    void Capture_DataAvailable(object sender, DataAvailableEventArgs e)
    {
        /*if (InvokeRequired)
        {
            MethodInvoker invoke = () => Capture_DataAvailable(sender, e);
            Invoke(invoke);
            return;
        }*/

        // The following data are available:

        // Scansion number:
        // e.ScanNumber             Samples per channel (data-scansion number)

        // Data buffers:
        // e.Samples                EMG samples
        // e.AccelerometerSamples   Accelerometer samples or IMU Accelerometer samples (available if it is an IMU raw data or IMU mixed data acquisition)
        // e.GyroscopeSamples       Gyroscope samples (available if it is an IMU raw data or IMU mixed data acquisition)
        // e.MagnetometerSamples    Magnetometer samples (available if it is an IMU raw data or IMU mixed data acquisition)
        // e.ImuSamples             Quaternions (available if it is an IMU fused data or IMU mixed data acquisition)
        // e.FootSwSamples          Foot-Sw samples
        // e.FootSwRawSamples       Foot-Sw raw samples

        // All the buffers store the same number of data-scansion
        // A sample in the buffer is significative if the corresponding sensor is enabled and the sensor type is properly configured

        // Triggers:
        //  e.StartTriggerDetected  True if start trigger was detected    
        //  e.StartTriggerScan      Number of the scansion corresponding to the start trigger detection   
        //  e.StopTriggerDetected   True if stop trigger was detected  
        //  e.StopTriggerScan       Number of the scansion corresponding to the stop trigger detection  

        // States:
        //  e.SensorStates          Sensor state (battery level). It is not available for inertial sensor if fused data capturing is enabled
        //  e.FootSwSensorStates    FootSw sensor state (battery level)

        //AcquiredSamplesPerChannelTextBox.Text = _acquiredSamplesPerChannel.ToString();
        //Debug.Log("Acquired samples per channel: " + e.ScanNumber);

        // Iterate through all acquired samples
        for (int kk = 0; kk < e.ScanNumber; ++kk)
        {
            // Check if trial is running
            if(UIManagerScript.isUserAllowedToMove)
            {
                // Add sample to EMG 1 buffer
                emg1[kk + bufferCounter] = e.Samples[4, kk];

                // Add sample to EMG 2 buffer
                emg2[kk + bufferCounter] = e.Samples[5, kk];

                // Add sample to EMG 3 buffer
                emg3[kk + bufferCounter] = e.Samples[6, kk];

                // Add sample to EMG 4 buffer
                emg4[kk + bufferCounter] = e.Samples[7, kk];

                trigger[kk + bufferCounter] = 0;

                dummies[kk + bufferCounter] = 0;
            }
        }
       
        // Use last emg value for visualization
        emgSensorValues[0] = emg1[e.ScanNumber - 1 + _acquiredSamplesPerChannel];
        emgSensorValues[1] = emg2[e.ScanNumber - 1 + _acquiredSamplesPerChannel];
        emgSensorValues[2] = emg3[e.ScanNumber - 1 + _acquiredSamplesPerChannel];
        emgSensorValues[3] = emg4[e.ScanNumber - 1 + _acquiredSamplesPerChannel];

        //Iterate through all IMU sensors
        for (int jj = 0; jj < nIMUSensors; ++jj)
        {
            // Iterate through all acquired samples
            for (int kk = 0; kk < e.ScanNumber; ++kk)
            {
                // Add IMU sensor data to string for sending
                Quaternion tempQuat = new Quaternion(e.ImuSamples[jj, 1, kk]/*quaternion component x*/,
                                                    e.ImuSamples[jj, 2, kk]/*quaternion component y*/,
                                                    e.ImuSamples[jj, 3, kk]/*quaternion component z*/,
                                                    e.ImuSamples[jj, 0, kk]/*quaternion component w*/);
                // Check if trial is running
                if (UIManagerScript.isUserAllowedToMove)
                {
                    switch (jj)
                    {
                        case 0: // IMU 1
                                // Add sample to IMU 1 buffer
                            imu1[kk + bufferCounter] = ConvertToUnity(tempQuat);
                            break;
                        case 1: // IMU 2
                                // Add sample to IMU 2 buffer
                            imu2[kk + bufferCounter] = ConvertToUnity(tempQuat);
                            break;
                        case 2: // IMU 3
                                // Add sample to IMU 3 buffer
                            imu3[kk + bufferCounter] = ConvertToUnity(tempQuat);
                            break;
                        case 3: // IMU 4
                                // Add sample to IMU 4 buffer
                            imu4[kk + bufferCounter] = ConvertToUnity(tempQuat);
                            break;
                    }
                }

                // If last sample
                if (kk == e.ScanNumber - 1)
                {
                    // Use sample for IMU visualization
                    imuSensorValues[jj] = ConvertToUnity(tempQuat);
                }
            }
        }

        // Show triggers status
        if (e.StartTriggerDetected)
        {
            //StartTriggerDetectedCheckBox.Checked = true;
            Debug.Log("Start trigger detected.");
            //StartTriggerScanTextBox.Text = e.StartTriggerScan.ToString();
            Debug.Log("Start trigger scan: " + e.StartTriggerScan.ToString());
            // Store sample where start trigger scan was detected
            trigger[bufferCounter + e.StartTriggerScan - 1] = 1;
            _daqSystem.GenerateInternalStopTrigger();
        }
        if (e.StopTriggerDetected)
        {

            //StopTriggerDetectedCheckBox.Checked = true;
            Debug.Log("Stop trigger detected.");
            //StopTriggerScanTextBox.Text = e.StopTriggerScan.ToString();
            Debug.Log("STop trigger scan: " + e.StopTriggerScan.ToString());
        }

        // Record dummy timing
        if(UIManagerScript.isUserAllowedToMove && UIManagerScript.shouldDummyBeRecorded)
        {
            // Store rough dummy timming
            dummies[bufferCounter] = 1;
            // Reset recording of dummy timing
            UIManagerScript.shouldDummyBeRecorded = false;
        }

        // Increase buffer if trial is running
        if (UIManagerScript.isUserAllowedToMove)
        {
            // Increase bufferCounter by newly acquired samples
            bufferCounter += e.ScanNumber;
        }

        byte state;
        // Show the sensors state (battery level)
        for (var c = 0; c < _installedSensor; c++)
        {
            // The battery level is not available for IMU sensors if fused-data acquisition is enabled
            state = (byte)(e.SensorStates[c, 0] & 0x03);
            //_sensorBatteryLevelBars[c].Value = state;
        }

        // Show the FootSw sensors state (battery level)
        state = (byte)(e.FootSwSensorStates[0, 0] & 0x03);
        //FswABatteryLevelProgressBar.Value = state;
        state = (byte)(e.FootSwSensorStates[1, 0] & 0x03);
        //FswBBatteryLevelProgressBar.Value = state;

        // Show acquired samples per channel
        _acquiredSamplesPerChannel += e.ScanNumber;
    }

    public void ResetTrial()
    {
        bufferCounter = 0;
        imu1 = new Quaternion[1200000];
        imu2 = new Quaternion[1200000];
        imu3 = new Quaternion[1200000];
        imu4 = new Quaternion[1200000];

        emg1 = new float[1200000];
        emg2 = new float[1200000];
        emg3 = new float[1200000];
        emg4 = new float[1200000];

        trigger = new int[1200000];
        dummies = new int[1200000];
    }

    Quaternion ConvertToUnity(Quaternion input)
    {
        return new Quaternion(
            -input.x,   // -(  right = -left  )
            -input.z,   // -(     up =  up     )
            -input.y,   // -(forward =  forward)
             input.w
        );
    }

    public void SaveDataAsync(int trialNumber)
    {
        // Get time and date
        string trialDateAndTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        // Create raw data, settings and angle filenames
        string rawDataFilename = trialDateAndTime + "_trialData" + trialNumber.ToString() + ".txt";
        string settingsFilename = trialDateAndTime + "_trialSettings" + trialNumber.ToString() + ".txt";
        string anglesFilename = trialDateAndTime + "_trialAngles" + trialNumber.ToString() + ".txt";

        // Specify precision of saved data
        int precision = 3;

        string formatString = "{0:G" + precision + //IMU1.x
            "}\t{1:G" + precision +     //IMU1.y
            "}\t{2:G" + precision +     //IMU1.z
            "}\t{3:G" + precision +     //IMU1.w
            "}\t{4:G" + precision +     //IMU2.x
            "}\t{5:G" + precision +     //IMU2.y
            "}\t{6:G" + precision +     //IMU2.z
            "}\t{7:G" + precision +     //IMU2.w
            "}\t{8:G" + precision +     //IMU3.x
            "}\t{9:G" + precision +     //IMU3.y
            "}\t{10:G" + precision +     //IMU3.z
            "}\t{11:G" + precision +     //IMU3.w
            "}\t{12:G" + precision +     //IMU4.x
            "}\t{13:G" + precision +     //IMU4.y
            "}\t{14:G" + precision +     //IMU4.z
            "}\t{15:G" + precision +     //IMU4.w
            "}\t{16:G" + precision +     //EMG1
            "}\t{17:G" + precision +     //EMG2
            "}\t{18:G" + precision +     //EMG3
            "}\t{19:G" + precision +     //EMG4
            "}\t{20:G" + precision +     //Trigger
            "}\t{21:G" + precision + "}";//Dummies

        // Open raw data file to write to
        using (var outf = new StreamWriter("C:\\Users\\Mathieu\\Downloads\\" + rawDataFilename))
        {
            // Write header to file
            outf.WriteLine("IMU1x\tIMU1y\tIMU1z\tIMU1w\tIMU2x\tIMU2y\tIMU2z\tIMU2w\tIMU3x\tIMU3y\tIMU3z\tIMU3w\tIMU4x\tIMU4y\tIMU4z\tIMU4w\tEMG1\tEMG2\tEMG3\tEMG4\tTrigger\tDummies");

            // For each buffer entry
            for (int i = 0; i < bufferCounter; i++)
            {
                // Write data sample to file
                outf.WriteLine(formatString, 
                    imu1[i].x.ToString(),
                    imu1[i].y.ToString(),
                    imu1[i].z.ToString(),
                    imu1[i].w.ToString(),
                    imu2[i].x.ToString(),
                    imu2[i].y.ToString(),
                    imu2[i].z.ToString(),
                    imu2[i].w.ToString(),
                    imu3[i].x.ToString(),
                    imu3[i].y.ToString(),
                    imu3[i].z.ToString(),
                    imu3[i].w.ToString(),
                    imu4[i].x.ToString(),
                    imu4[i].y.ToString(),
                    imu4[i].z.ToString(),
                    imu4[i].w.ToString(),
                    emg1[i].ToString(),
                    emg2[i].ToString(),
                    emg3[i].ToString(),
                    emg4[i].ToString(),
                    trigger[i].ToString(),
                    dummies[i].ToString());
            }
        }

        // Save settings to separate file
        uiScript.SaveSettingsAsync(settingsFilename);

        // Save IMU angles to separate file
        SaveAnglesAsync(anglesFilename);

        // Reset buffer variables for storing next trial
        ResetTrial();
    }

    public void SaveAnglesAsync(string filename)
    {
        // Specify precision of saved data
        int precision = 3;

        string angleFormatString = "{0:G" + precision + //IMU1.rotx
            "}\t{1:G" + precision +     //IMU1.roty
            "}\t{2:G" + precision +     //IMU1.rotz
            "}\t{3:G" + precision +     //IMU2.rotx
            "}\t{4:G" + precision +     //IMU2.roty
            "}\t{5:G" + precision +     //IMU2.rotz
            "}\t{6:G" + precision +     //IMU3.rotx
            "}\t{7:G" + precision +     //IMU3.roty
            "}\t{8:G" + precision +     //IMU3.rotz
            "}\t{9:G" + precision +     //IMU4.rotx
            "}\t{10:G" + precision +     //IMU4.roty
            "}\t{11:G" + precision + "}";//IMU4.rotz

        // Open raw data file to write to
        using (var outf = new StreamWriter("C:\\Users\\Mathieu\\Downloads\\" + filename))
        {
            // Write header to file
            outf.WriteLine("IMU1rotx\tIMU1roty\tIMU1rotz\tIMU2rotx\tIMU2roty\tIMU2rotz\tIMU3rotx\tIMU3roty\tIMU3rotz\tIMU4rotx\tIMU4roty\tIMU4rotz");

            // For each buffer entry
            for (int jj = 0; jj < bufferCounter; jj++)
            {
                // Write data sample to file
                outf.WriteLine(angleFormatString,
                    imu1[jj].eulerAngles.x.ToString(),
                    imu1[jj].eulerAngles.y.ToString(),
                    imu1[jj].eulerAngles.z.ToString(),
                    imu2[jj].eulerAngles.x.ToString(),
                    imu2[jj].eulerAngles.y.ToString(),
                    imu2[jj].eulerAngles.z.ToString(),
                    imu3[jj].eulerAngles.x.ToString(),
                    imu3[jj].eulerAngles.y.ToString(),
                    imu3[jj].eulerAngles.z.ToString(),
                    imu4[jj].eulerAngles.x.ToString(),
                    imu4[jj].eulerAngles.y.ToString(),
                    imu4[jj].eulerAngles.z.ToString());
            }
        }
    }

    void SensorMemory_DataAvailable(object sender, SensorMemoryDataAvailableEventArgs e)
    {
        /*if (InvokeRequired)
        {
            MethodInvoker invoke = () => SensorMemory_DataAvailable(sender, e);
            Invoke(invoke);
            return;
        }*/

        // The following data are available:

        // Scansion number:
        // e.SamplesNumber                  Samples per channel (data-scansion number)

        // Data buffers:
        // e.Samples                        EMG samples
        // e.AccelerometerSamples           Accelerometer samples or IMU Accelerometer samples (available if it is an IMU raw data or IMU mixed data acquisition)
        // e.GyroscopeSamples               Gyroscope samples (available if it is an IMU raw data or IMU mixed data acquisition)
        // e.MagnetometerSamples            Magnetometer samples (available if it is an IMU raw data or IMU mixed data acquisition)
        // e.ImuSamples                     Quaternions (available if it is an IMU fused data or IMU mixed data acquisition)
        // e.FootSwSamples                  Foot-Sw samples
        // e.FootSwRawSamples               Foot-Sw raw samples

        // All the buffers store the same number of data-scansion
        // A sample in the buffer is significative if the corresponding sensor is enabled and the sensor type is properly configured

        // e.SavedTrialsNumber              Number of trials saved in the sensor memory
        // e.TransferredSamplesInPercent    Imported samples in percent
        // e.TrialEnd                       True if all the samples of the current trial have been imported

        // States:
        //  e.SensorStates                  Sensor state (battery level). It is not available for inertial sensor if fused data capturing is enabled
        //  e.FootSwSensorStates            FootSw sensor state (battery level)

        // Show acquired samples per channel
        _acquiredSamplesPerChannel += e.SamplesNumber;
        //ImportedSamplesPerChannelTextBox.Text = _acquiredSamplesPerChannel.ToString();
        //Debug.Log("Acquired samples per channel: " + _acquiredSamplesPerChannel.ToString());

        // Show importing process progress
        //MemoryImportingProgressBar.Value = e.TransferredSamplesInPercent <= MemoryImportingProgressBar.Maximum ? e.TransferredSamplesInPercent : MemoryImportingProgressBar.Maximum;

        // Check if the current trial has been completely imported
        if (e.TrialEnd)
        {
            _importedTrials++;
            //ImportedTrialsTextBox.Text = _importedTrials.ToString();
            Debug.Log("Imported Trials: " + _importedTrials.ToString());
        }

        // Show the sensors state (battery level)
        byte state;
        for (var c = 0; c < _installedSensor; c++)
        {
            // The battery level is not available for IMU sensors if fused-data acquisition is enabled
            state = (byte)(e.SensorStates[c, 0] & 0x03);
            //_sensorBatteryLevelBars[c].Value = state;
        }

        // Show the FootSw sensors state (battery level)
        state = (byte)(e.FootSwSensorStates[0, 0] & 0x03);
        //FswABatteryLevelProgressBar.Value = state;
        state = (byte)(e.FootSwSensorStates[1, 0] & 0x03);
        //FswBBatteryLevelProgressBar.Value = state;
    }

    private void getFirmwareVersion()
    {
        try
        {
            var versionList = _daqSystem.FirmwareVersion;
            // Clear firmware version controls
            //FwVersionTextBox.Text = "";
            string fwVersion = "";
            // Get device firmware version
            foreach (var version in versionList)
            {
                //FwVersionTextBox.Text = FwVersionTextBox.Text + version.Major + "." + version.Minor + "    ";
                fwVersion = fwVersion + version.Major + "." + version.Minor + "    ";
            }
            Debug.Log("Firmware Version: " + fwVersion);
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void getHardwareVersion()
    {
        try
        {
            var versionList = _daqSystem.HardwareVersion;
            // Clear Hw version controls
            //HwVersionTextBox.Text = "";
            string hwVersion = "";
            // Get device Hw version
            foreach (var version in versionList)
            {
                //HwVersionTextBox.Text = HwVersionTextBox.Text + version.Major + "." + version.Minor + "    ";
                hwVersion = hwVersion + version.Major + "." + version.Minor + "    ";
            }
            Debug.Log("Hardware Version: " + hwVersion);
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void getSoftwareVersion()
    {
        try
        {
            var version = _daqSystem.SoftwareVersion;
            // Get device Sw version
            //SwVersionTextBox.Text = version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
            string swVersion = version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
            Debug.Log("Software Version: " + swVersion);
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void getInstalledSensors()
    {
        try
        {
            // Get installed sensors number
            _installedSensor = _daqSystem.InstalledSensors;
            //InstalledSensorsTextBox.Text = _installedSensor.ToString();
            Debug.Log("# of installed sensors: " + _installedSensor.ToString());

            /*// Set sensors state controls visibility
            for (var c = 0; c < DeviceConstant.MAX_SENSORS_NUM; c++)
            {
                _sensorBatteryLevelBars[c].Visible = c < _installedSensor;
                _sensorNumberLabels[c].Visible = c < _installedSensor;
            }*/
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void getInstalledFootSwSensors()
    {
        try
        {
            // Get installed footSw sensors number
            //InstalledFootSwSensorsTextBox.Text = _daqSystem.InstalledFootSwSensors.ToString();
            Debug.Log("Foot switch sensor number: " + _daqSystem.InstalledFootSwSensors.ToString());
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    /*private void BtnConfigureCaptureAndSensors_Click(object sender, EventArgs e)
    {
        try
        {
            // Create and show the "Capture and Sensors Configuration" form
            _captureAndSensorsConfigForm = new CaptureAndSensorConfigForm(_daqSystem);
            _daqSystem.StateChanged += _captureAndSensorsConfigForm.Device_StateChanged;
            _captureAndSensorsConfigForm.Show();
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
    }*/

    public void ToggleCapture(bool toggleValue)
    {
        if(toggleValue)
        {
            Debug.Log("Is capturing.");
            StartCapture_Button();
        }
        else
        {
            Debug.Log("Stopped capturing.");
            StopCapture_Button();
        }
    }

    // Update is called once per frame
    public void StartCapture_Button()
    {
        Debug.Log("Start clicked");
        try
        {
            // Configure sensor 1-4 as Inertial sensor (accelerometer full scale = 8g; gyroscope full scale = 2000dps)
            ISensorConfiguration sensorConfiguration = new SensorConfiguration
            {
                SensorType = SensorType.INERTIAL_SENSOR,
                AccelerometerFullScale = AccelerometerFullScale.g_8,
                GyroscopeFullScale = GyroscopeFullScale.dps_2000
            };
            for (var c = 1; c <= 4; c++)
            {
                _daqSystem.ConfigureSensor(sensorConfiguration, c);
            }
                
            // Configure sensors from 5 to _installedSensor as EMG sensors (accelerometer full scale = 2g)
            sensorConfiguration.SensorType = SensorType.EMG_SENSOR;
            sensorConfiguration.AccelerometerFullScale = AccelerometerFullScale.g_2;
            for (var c = 5; c <= 8 ; c++)
                _daqSystem.ConfigureSensor(sensorConfiguration, c);

            //StartTriggerDetectedCheckBox.Checked = false;
            //StartTriggerScanTextBox.Text = "";
            //StopTriggerDetectedCheckBox.Checked = false;
            //StopTriggerScanTextBox.Text = "";
            _acquiredSamplesPerChannel = 0;

            // Get DataAvailableEvent period
            //var dataAvailableEventPeriod = ((DataAvailableEventPeriod)Enum.Parse(typeof(DataAvailableEventPeriod), comboBoxDataAvailableEventPeriod.SelectedIndex.ToString()));
            var dataAvailableEventPeriod = DataAvailableEventPeriod.ms_10; //fixed capturing interval of 10ms
            // Start data acquisition
            _daqSystem.StartCapturing(dataAvailableEventPeriod);
            _isAcquiring = true;
            //recButton.interactable = true;
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    public void StopCapture_Button()
    {
        try
        {
            // Stop data capture process
            _daqSystem.StopCapturing();
            _isAcquiring = false;
            //recButton.interactable = false;
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    public void GenerateStartTrigger_Button()
    {
        try
        {
            // Generate start trigger by software 
            _daqSystem.GenerateInternalStartTrigger();
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void GenerateStopTriggerButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Generate stop trigger by software 
            _daqSystem.GenerateInternalStopTrigger();
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void DetectAccelerometerOffsetButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Detect and hardware-compensate the selected accelerometer offset
            _daqSystem.DetectAccelerometerOffset(0);
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void FirmwareVersionButton_Click(object sender, EventArgs e)
    {
        // Get device firmware version
        getFirmwareVersion();
    }

    private void HardwareVersionButton_Click(object sender, EventArgs e)
    {
        // Get device hardware version
        getHardwareVersion();
    }

    private void SoftwareVersionButton_Click(object sender, EventArgs e)
    {
        // Get Daq software version
        getSoftwareVersion();
    }

    private void GetInstalledSensorsButton_Click(object sender, EventArgs e)
    {
        // Get the device installed sensors number
        getInstalledSensors();
    }

    private void GetInstalledFootSwSensorsButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Get installed sensors number
            //InstalledSensorsTextBox.Text = _daqSystem.InstalledSensors.ToString();
            Debug.Log("# of installed sensors: " + _daqSystem.InstalledSensors.ToString());
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    public void CalibrateImuSensorOffset_Button()
    {
        try
        {
            // Calibrate the selected Imu sensor offset
            _daqSystem.CalibrateSensorImuOffset(0);
        }
        catch (Exception _exception)
        {
            // Show exception message
            Debug.Log(_exception.Message);
        }
    }

    private void ChangeRFChannelReadDeviceCurrentButton_Click(object sender, EventArgs e)
    {
        // Get master device RF channel
        getMasterDeviceRFChannel();
    }

    private void getMasterDeviceRFChannel()
    {
        // Get master device RF channel
        var rfChannel = _daqSystem.DeviceRFChannel(0);
        //ChangeMasterRFChannelCurrentTextBox.Text = rfChannel.ToString();
        Debug.Log("Master device RF channel: " + rfChannel.ToString());
    }
    
    /*private void ChangeRFChannelChangeSensorsButton_Click(object sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            // Set RF channel of all the master device sensors
            var rfChannel = ((RFChannel)Enum.Parse(typeof(RFChannel), ChangeMasterRFChannelNewComboBox.SelectedIndex.ToString()));
            _daqSystem.ChangeSensorsRFChannel(rfChannel, 0);
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }

    }

    private void ChangeMasterRFChannelChangeDeviceButton_Click(object sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            // Set RF channel of RX master device
            var rfChannel = ((RFChannel)Enum.Parse(typeof(RFChannel), ChangeMasterRFChannelNewComboBox.SelectedIndex.ToString()));
            _daqSystem.ChangeDeviceRFChannel(rfChannel, 0);
            // The Receiver must be restarted 
            MessageBox.Show("Restart the Receiver for the changes to take effect");
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void MemoryModeEnableCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            if (MemoryModeEnableCheckBox.Checked)
            {
                CaptureGroupBox.Enabled = false;
                SensorMemoryGroupBox.Enabled = false;
                Application.DoEvents();
                // Put the sensors and the receiver in memory mode
                _daqSystem.EnableSensorMemoryMode();
                RemoteRecordingPanel.Enabled = true;
            }
            else
            {
                CaptureGroupBox.Enabled = true;
                SensorMemoryGroupBox.Enabled = true;
                RemoteRecordingPanel.Enabled = false;
                Application.DoEvents();
                // Disable sensor memory mode
                _daqSystem.DisableSensorMemoryMode();
                Thread.Sleep(3000);
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void StartRemoteRecordingButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (_daqSystem.State != DeviceState.NotConnected)
            {
                Cursor = Cursors.WaitCursor;
                // Start sensor memory recording
                _daqSystem.StartSensorMemoryRecording(1);
                Cursor = Cursors.Default;
                Thread.Sleep(200);
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void StopRemoteRecordingButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (_daqSystem.State != DeviceState.NotConnected)
            {
                Cursor = Cursors.WaitCursor;
                // Stop sensor memory recording
                _daqSystem.StopSensorMemoryRecording();
                Cursor = Cursors.Default;
                Thread.Sleep(200);
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void GetMemoryStatusButton_Click(object sender, EventArgs e)
    {
        if (_daqSystem.State != DeviceState.NotConnected)
        {
            // Get and show sensor memory status
            var sensorMemoryStatus = _daqSystem.SensorsMemoryStatus();
            if (sensorMemoryStatus != null)
            {
                MemoryTrialsNumberTextBox.Text = sensorMemoryStatus.SavedTrialsNumber.ToString();
                MemoryUsedSpaceTextBox.Text = sensorMemoryStatus.UsedMemorySpaceInPercent.ToString();
                MemoryAvailableTimeTextBox.Text = (sensorMemoryStatus.ResidualRecTime_sec / 60).ToString();
            }
        }
    }

    private void MemoryClearButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (_daqSystem.State != DeviceState.NotConnected)
            {
                Cursor = Cursors.WaitCursor;
                // Clear all sensors memory
                _daqSystem.ClearSensorMemory(0);
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void MemoryStopImportingButton_Click(object sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            // Stop sensor memory reading
            if (_daqSystem.State == DeviceState.ReadingSensorMemory)
            {
                _daqSystem.StopSensorMemoryReading();
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void MemoryStartImportingButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Configure sensor 1 as Inertial sensor (accelerometer full scale = 8g; gyroscope full scale = 2000dps)
            ISensorConfiguration sensorConfiguration = new SensorConfiguration
            {
                SensorType = SensorType.INERTIAL_SENSOR,
                AccelerometerFullScale = AccelerometerFullScale.g_8,
                GyroscopeFullScale = GyroscopeFullScale.dps_2000
            };
            _daqSystem.ConfigureSensor(sensorConfiguration, 1);

            // Configure sensors 2, 3 and from 4 to _installedSensor as EMG sensors (accelerometer full scale = 2g)
            sensorConfiguration.SensorType = SensorType.EMG_SENSOR;
            sensorConfiguration.AccelerometerFullScale = AccelerometerFullScale.g_2;
            _daqSystem.ConfigureSensor(sensorConfiguration, 2);
            _daqSystem.ConfigureSensor(sensorConfiguration, 3);
            for (var c = 4; c <= _installedSensor; c++)
                _daqSystem.ConfigureSensor(sensorConfiguration, c);

            _acquiredSamplesPerChannel = 0;
            _importedTrials = 0;
            ImportedTrialsTextBox.Text = "0";
            ImportedSamplesPerChannelTextBox.Text = "0";
            MemoryImportingProgressBar.Value = 0;
            Cursor = Cursors.WaitCursor;
            // Start sensor memory reading
            if (_daqSystem.State == DeviceState.Idle)
            {
                _daqSystem.StartSensorMemoryReading();
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void MemoryStartSelectiveImportingButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Configure sensor 1 as Inertial sensor (accelerometer full scale = 8g; gyroscope full scale = 2000dps)
            ISensorConfiguration sensorConfiguration = new SensorConfiguration
            {
                SensorType = SensorType.INERTIAL_SENSOR,
                AccelerometerFullScale = AccelerometerFullScale.g_8,
                GyroscopeFullScale = GyroscopeFullScale.dps_2000
            };
            _daqSystem.ConfigureSensor(sensorConfiguration, 1);

            // Configure sensors 2, 3 and from 4 to _installedSensor as EMG sensors (accelerometer full scale = 2g)
            sensorConfiguration.SensorType = SensorType.EMG_SENSOR;
            sensorConfiguration.AccelerometerFullScale = AccelerometerFullScale.g_2;
            _daqSystem.ConfigureSensor(sensorConfiguration, 2);
            _daqSystem.ConfigureSensor(sensorConfiguration, 3);
            for (var c = 4; c <= _installedSensor; c++)
                _daqSystem.ConfigureSensor(sensorConfiguration, c);

            _acquiredSamplesPerChannel = 0;
            _importedTrials = 0;
            ImportedTrialsTextBox.Text = "0";
            ImportedSamplesPerChannelTextBox.Text = "0";
            MemoryImportingProgressBar.Value = 0;
            Cursor = Cursors.WaitCursor;
            // Start sensor memory reading
            if (_daqSystem.State == DeviceState.Idle)
            {
                // Start trial 1 memory selective importing
                _daqSystem.StartSensorSelectiveMemoryReading(1);
            }
        }
        catch (Exception _exception)
        {
            // Show exception message
            MessageBox.Show(_exception.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }*/
}
