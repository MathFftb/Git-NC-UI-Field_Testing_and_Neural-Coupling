using UnityEngine;
using LabJack;
using TMPro;

// Using a Labjack T7

public class LabJackManager : MonoBehaviour
{
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


    public TMP_Text displayEntry;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeValues();
        ConnectLabJack();
        MainLoop();
        DisconnectLabJack();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MainLoop()
    {
        // Main loop: Read Every Second
        
        int it = 0; //Loop counter; incremented later.
        double dacVolt = 0.0;
        int fioState = 0;
        Debug.Log("\nStarting read loop.");

        /// StartInterval() Method:
        /// Sets up a reoccurring interval timer
        /// Interval is based on the host clock.
        /// Interval keeps reoccuring.
        /// Can be used with WaitForNextInterval(see inside while loop): 
        ///     wait/blocks/sleeps until next interval occurence
        LJM.StartInterval(intervalHandle, 1000000);

        // While Loop:
        // 1. While statement: Lets the example keep running until you tap any key—a simple, cross‑platform “stop button”.
        //while (!Console.KeyAvailable) //: Console.KeyAvailable: becomes true when the user has pressed a key that hasn’t been read yet.
        int iterations = 0;
        while (iterations < 10)
        {
            /// 2. Choose which registers to write to 
            /// using eWriteNames()
            /// On T7/T8 the first digital I/O (FIO1) is used.
            /// Both always pair with the analog output DAC0.
            //DAC0 will cycle ~0.0 to ~5.0 volts in 1.0 volt increments.
            //FIO5/FIO1 will toggle output high (1) and low (0) states.

            /*

            if (devType == LJM.CONSTANTS.dtT4)
            {
                aNames = new string[] { "DAC0", "FIO5" };
            }
            else
            {
                aNames = new string[] { "DAC0", "FIO1" };
            }

            // 3. Generate values to send
            dacVolt = it % 6.0;  //0-5
            fioState = it % 2;  //0 or 1
            aValues = new double[] { dacVolt, (double)fioState };
            numFrames = aNames.Length;
            

            // 4. Write the values to the hardware
            LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errorAddress);

            // 5. Log what was written
            Debug.Log("\neWriteNames :");
            for (int i = 0; i < numFrames; i++)
                Debug.Log(" " + aNames[i] + " = " + aValues[i].ToString("F4") + ", ");
            Debug.Log("");

            */

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

            // 7. Read the values
            LJM.eReadNames(handle, numFrames, aNames, aValues, ref errorAddress);

            // 8. Log what was read
            Debug.Log("eReadNames  :");
            for (int i = 0; i < numFrames; i++)
                Debug.Log(" " + aNames[i] + " = " + aValues[i].ToString("F4") + ", ");
            Debug.Log("");

            // 8b. Write the entry in a TMP display in the Unity UI.
            displayEntry.text = aNames[1] + "=" + aValues[1].ToString("F4");

            // 9. Housekeeping for next iteration
            it++;

            // 10. Fixed‑rate timing
            //Wait for next 1 second interval
            LJM.WaitForNextInterval(intervalHandle, ref skippedIntervals);
            if (skippedIntervals > 0)
            {
                Debug.Log("SkippedIntervals: " + skippedIntervals);
            }
            // 11. Loop ends

            ++iterations;
            Debug.Log(iterations);
        }
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
    }

    public void ConfigLabJack()
    {
        //LabJack T7 and T8 configuration

        //Settling and negative channel do not apply to the T8
        if (devType == LJM.CONSTANTS.dtT7)
        {
            // Here: configures analog input for single-ended mode (only AIN0, ground-referenced).
            aNames = new string[] { "AIN0_NEGATIVE_CH",
                                    "AIN0_SETTLING_US"};

            // Negative Channel = 199 (Single-ended): 199 means no default is used, only AIN0
            // Settling = 0 (auto); settling time = time taken by system output to stabilize within a specific range after disturbance or input change
            aValues = new double[] { 199, 0 };
            // By definition numFrames = size of aNames
            numFrames = aNames.Length;

            /// eWriteNames: Write multiple device registers  in one command, each register specified by name in "aNames":
            /// (device registers = hardware registers within the CPU at fast small memory locations)
            /// Parameters: 
            /// handle: A device handle. The handle is a connection ID for an active device. 
            ///         Generate a handle with LJM_Open or LJM_OpenS.
            /// numFrames: The total number of frames to access. 
            ///             A frame consists of one value, so the number of frames is the size of the aNames array.
            /// aNames: An array of names that specify the Modbus register(s) to write. 
            ///         Names can be found throughout the device datasheet or in the Modbus Map.
            /// aValues: An array of values to send to the device. The array size should be the same as the aNames array. 
            ///          The input data type of each value is a double, and will be converted into the correct data type automatically.
            /// errorAddress: If error, the address responsible for causing an error.
            LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errorAddress);
        }

        /// This configures T7 so that analog input on AIN0 is ready to read voltages—which is exactly where you'd connect the torque sensor output.
        /// AIN0(analog input 0): 
        //    Range = ±10V (T7) or ±11V (T8).
        //    Resolution index = 0 (default).
        aNames = new string[] { "AIN0_RANGE",
                                    "AIN0_RESOLUTION_INDEX"};
        aValues = new double[] { 10,   //  Range ±10V: full voltage swing allowed.
                                        0 }; //  Resolution index 0: lowest (fastest) resolution.
        numFrames = aNames.Length;
        // Same memory allocation as above
        LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errorAddress);

    }

    public void DisconnectLabJack()
    {
        //LJM.CleanInterval(intervalHandle);
        LJM.CloseAll();
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

        Debug.Log("Values Initialized");
    }
}
