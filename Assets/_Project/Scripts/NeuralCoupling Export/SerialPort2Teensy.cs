using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using TMPro; 

public class SerialPort2Teensy : MonoBehaviour
{
    // Reference to port selector dropdown
    public Dropdown portDropDown;
    public string serialPortName = "";
    public WaveplusDaqScript wavePlusInstance;

    private SerialPort sp;

    // Start is called before the first frame update
    void Start()
    {
        // Get a list of serial port names
        //List<string> ports = SerialPort.GetPortNames().ToList();

        // Check if any ports were detected
        /*if(ports.Count > 0)
        {
            // Clear default dropdown options        
            portDropDown.ClearOptions();
            // Add list of ports as dropdown options
            portDropDown.AddOptions(ports);

            //Add listener for when the value of the Dropdown changes, to take action
            portDropDown.onValueChanged.AddListener(delegate {
                PortDropdownValueChanged();
            });
        }
        else
        {
            Debug.Log("No COM ports found.");
        }*/

        //Debug: Display each port name to the console.
        //foreach (string port in ports)
        //{
        //    Debug.Log(port);
        //}

        serialPortName = "COM5";
        OpenConnection();
    }

    public void PortDropdownValueChanged()
    {
        serialPortName = portDropDown.options[portDropDown.value].text;
        OpenConnection();
    }

    public void OpenConnection()
    {
        // Check if an existing serial port was selected
        if(serialPortName != "")
        {
            try
            {
                // Create serial port object with standard baudrate
                sp = new SerialPort(serialPortName, 9600);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            // Check if serial port exists
            if (sp != null)
            {
                // Check if port is open
                if (sp.IsOpen)
                {
                    //sp.Close();
                    Debug.Log("Port already open.");
                }
                else
                {
                    // Open port
                    sp.Open();
                    // Set read timeout
                    sp.ReadTimeout = 100;
                    Debug.Log("Port opened.");
                }
            }
            else
            {
                Debug.Log("Port is not set.");
            }
        }
    }

    void OnApplicationQuit()
    {
        // Check if port exists and is open
        if(sp != null && sp.IsOpen)
        {
            // Close port
            sp.Close();
        }
    }


    public void triggerStimulation()
    {
        if(sp != null && sp.IsOpen)
        {
            // Send stimulation trigger to Teensy
            sp.Write("1");
            Debug.Log("Stimulation triggered.");

            // Trigger internal start trigger
            wavePlusInstance._daqSystem.GenerateInternalStartTrigger();
        }
        else
        {
            Debug.LogError("No serial port connected. Stimulation canceled");
        }
        
    }
}