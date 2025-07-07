using UnityEngine;
/*
public class WriteReadLoopWithConfig : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
}
*/
//-----------------------------------------------------------------------------
// WriteReadLoopWithConfig.cs
//
// Performs an initial call to eWriteNames to write configuration values, and
// then calls eWriteNames and eReadNames repeatedly in a loop.
//
// support@labjack.com
//
// Relevant Documentation:
//
// LJM Library:
//     LJM Library Installer:
//         https://labjack.com/support/software/installers/ljm
//     LJM Users Guide:
//         https://labjack.com/support/software/api/ljm
//     Opening and Closing:
//         https://labjack.com/support/software/api/ljm/function-reference/opening-and-closing
//     Multiple Value Functions (such as eWriteNames and eReadNames):
//         https://labjack.com/support/software/api/ljm/function-reference/multiple-value-functions
//     Timing Functions (such as StartInterval, WaitForNextInterval and
//     CleanInterval):
//         https://labjack.com/support/software/api/ljm/function-reference/timing-functions
//
// T-Series and I/O:
//     Modbus Map:
//         https://labjack.com/support/software/api/modbus/modbus-map
//     Analog Inputs:
//         https://labjack.com/support/datasheets/t-series/ain
//     Digital I/O:
//         https://labjack.com/support/datasheets/t-series/digital-io
//     DAC:
//         https://labjack.com/support/datasheets/t-series/dac
//-----------------------------------------------------------------------------
using System;
using LabJack;
using Unity;
using TMPro;
using UnityEngine.UI;
using System.CodeDom;


namespace WriteReadLoopWithConfig
{
    class WriteReadLoopWithConfig : MonoBehaviour
    {
        public TMP_Text displayEntry;

        // For console testing
        /*static void Main(string[] args)
        {
            WriteReadLoopWithConfig wrlwc = new WriteReadLoopWithConfig();
            wrlwc.performActions();
        }*/

        void Start()
        {
            //WriteReadLoopWithConfig wrlwc = new WriteReadLoopWithConfig();
            this.performActions();
        }

        public void showErrorMessage(LJM.LJMException e)
        {
            Console.Out.WriteLine("LJMException: " + e.ToString());
            Console.Out.WriteLine(e.StackTrace);
        }

        // This is the core method where everything happens: device connection, setup, reading/writing, and looping.
        public void performActions()
        {
            int handle = 0;
            int devType = 0; // Device type (T4, T7, T8)
            int conType = 0; // USB, Ethernet, WiFi
            int serNum = 0;  // Serial number
            int ipAddr = 0;  // IP address (numeric form)
            int port = 0;
            int maxBytesPerMB = 0;
            string ipAddrStr = "";
            int numFrames = 0;
            string[] aNames; // array of strings there to be filled and passed for command in this code
            double[] aValues; // array of doubles there to be filled and passed for command in this code
            int errorAddress = -1;
            int intervalHandle = 1; // For timed reading every second
            int skippedIntervals = 0;

            // Connecting to LabJack
            try
            {
                //Open first found LabJack
                // Opens any available LabJack device.
                // "handle" is a reference to this open device – used for all further communication.
                LJM.OpenS("ANY", "ANY", "ANY", ref handle);  // Any device, Any connection, Any identifier
                                                             //LJM.OpenS("T8", "ANY", "ANY", ref handle);  // T8 device, Any connection, Any identifier
                                                             //LJM.OpenS("T7", "ANY", "ANY", ref handle);  // T7 device, Any connection, Any identifier
                                                             //LJM.OpenS("T4", "ANY", "ANY", ref handle);  // T4 device, Any connection, Any identifier
                                                             //LJM.Open(LJM.CONSTANTS.dtANY, LJM.CONSTANTS.ctANY, "ANY", ref handle);  // Any device, Any connection, Any identifier

                // Get and Display Device Info
                // Gets type, connection method, serial number, etc.
                LJM.GetHandleInfo(handle, ref devType, ref conType, ref serNum, ref ipAddr, ref port, ref maxBytesPerMB);
                // Converts numeric IP to a readable string.
                LJM.NumberToIP(ipAddr, ref ipAddrStr);

                Debug.Log("Opened a LabJack with Device type: " + devType + ", Connection type: " + conType + ",");
                Debug.Log("  Serial number: " + serNum + ", IP address: " + ipAddrStr + ", Port: " + port + ",");
                Debug.Log("  Max bytes per MB: " + maxBytesPerMB);

                //Setup and call eWriteNames to configure AIN0 (all devices)
                //and digital I/O (T4 only)
                if (devType == LJM.CONSTANTS.dtT4) // When using LabJack T4 model (not this experiment normally)
                {
                    //LabJack T4 configuration

                    //Set FIO5 (DIO5) and FIO6 (DIO6) lines to digital I/O.
                    //    DIO_INHIBIT = 0xF9F, b111110011111.
                    //                  Update only DIO5 and DIO6.
                    //    DIO_ANALOG_ENABLE = 0x000, b000000000000.
                    //                        Set DIO5 and DIO6 to digital I/O (b0).
                    //    Resolution index = 0 (default)
                    //    Settling = 0 (auto)
                    aNames = new string[] { "DIO_INHIBIT", "DIO_ANALOG_ENABLE",
                                            "AIN0_RESOLUTION_INDEX", "AIN0_SETTLING_US" };
                    aValues = new double[] { 0xF9F, 0x000, 0, 0 };
                    numFrames = aNames.Length;
                    LJM.eWriteNames(handle, numFrames, aNames, aValues, ref errorAddress);
                }
                else // Using LabJack T7 model(our case)
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

                // Main loop: Write + Read Every Second
                
                int it = 0; //Loop counter; incremented later.
                double dacVolt = 0.0;
                int fioState = 0;
                Debug.Log("\nStarting read loop.  Press a key to stop.");

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
                    /// On a T4 the digital I/O labelled FIO5 is chosen.
                    /// On T7/T8 the first digital I/O (FIO1) is used.
                    /// Both always pair with the analog output DAC0.
                    //DAC0 will cycle ~0.0 to ~5.0 volts in 1.0 volt increments.
                    //FIO5/FIO1 will toggle output high (1) and low (0) states.
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
            catch (LJM.LJMException e)
            {
                showErrorMessage(e);
            }

            //Close interval and device handles
            LJM.CleanInterval(intervalHandle);
            LJM.CloseAll();

            Debug.Log("\nDone.\nPress the enter key to exit.");
            Console.ReadLine();  //Pause for user
        }
    }
}