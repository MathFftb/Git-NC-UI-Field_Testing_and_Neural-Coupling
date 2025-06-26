using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Globalization;
using UnityEngine.UI;

public class UDPClient : MonoBehaviour
{
    public IPAddress localhost = IPAddress.Parse("127.0.0.1");
    public int port = 11000;
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

    private UdpClient client;
    private IPEndPoint remoteEP;
    private int floatSize;
    private Thread listenerThread;
    private static UDPClient instance = null;

    private float maxAngle = -Mathf.Infinity;
    private float minAngle = Mathf.Infinity;
    private bool isCalibrating = false;

    private Color stdCalibColor;

    private void Awake()
    {
        // If the udp client instance is created for the first time
        if (instance == null)
        {
            // Store reference
            instance = this;
            // Prevent it from being destroyed when scene is reloaded
            DontDestroyOnLoad(gameObject);
        }
        // A udp client instance already exists
        else if (instance != this)
        {
            // Destroy the newly created instance
            Destroy(this.gameObject);
            return;
        }

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

    void Start()
    {
        // Start UDPServer background thread 		
        ////listenerThread = new Thread(new ThreadStart(Listen4IMUData))
        ////{
        ////    IsBackground = true
        ////};
        ////listenerThread.Start();
        StartReceiving();
    }

    //private void Listen4IMUData()
    //{
    //    try
    //    {
    //        StartReceiving();
    //    }
    //    catch (SystemException exception)
    //    {
    //        Debug.Log(exception);
    //    }
    //}

    public void StartReceiving() 
    {
        remoteEP = new IPEndPoint(localhost, port); //IPAddress.Any, port
        client = new UdpClient(remoteEP);
        Receive(); // initial start of our "loop"
    }

    public void StopReceiving() 
    {
        stop = true;    
        client.Close();
    }

    private void Receive() 
    {   
        client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    private void ReceiveCallback(IAsyncResult result) 
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = client.EndReceive(result, ref ip);

        string receivedString = Encoding.ASCII.GetString(receivedBytes, 0, receivedBytes.Length);

        // Parse received string to IMU quaternions
        string[] stringData = receivedString.Split(',');

        emgData[0] = float.Parse(stringData[0], CultureInfo.InvariantCulture);
        emgData[1] = float.Parse(stringData[1], CultureInfo.InvariantCulture);
        emgData[2] = float.Parse(stringData[2], CultureInfo.InvariantCulture);
        emgData[3] = float.Parse(stringData[3], CultureInfo.InvariantCulture);

        quaternionData[0] = ConvertToUnity(new Quaternion(float.Parse(stringData[4], CultureInfo.InvariantCulture),
            float.Parse(stringData[5], CultureInfo.InvariantCulture),
            float.Parse(stringData[6], CultureInfo.InvariantCulture),
            float.Parse(stringData[7], CultureInfo.InvariantCulture)));

        quaternionData[1] = ConvertToUnity(new Quaternion(float.Parse(stringData[8], CultureInfo.InvariantCulture),
            float.Parse(stringData[9], CultureInfo.InvariantCulture),
            float.Parse(stringData[10], CultureInfo.InvariantCulture),
            float.Parse(stringData[11], CultureInfo.InvariantCulture)));

        quaternionData[2] = ConvertToUnity(new Quaternion(float.Parse(stringData[12], CultureInfo.InvariantCulture),
            float.Parse(stringData[13], CultureInfo.InvariantCulture),
            float.Parse(stringData[14], CultureInfo.InvariantCulture),
            float.Parse(stringData[15], CultureInfo.InvariantCulture)));

        quaternionData[3] = ConvertToUnity(new Quaternion(float.Parse(stringData[16], CultureInfo.InvariantCulture),
            float.Parse(stringData[17], CultureInfo.InvariantCulture),
            float.Parse(stringData[18], CultureInfo.InvariantCulture),
            float.Parse(stringData[19], CultureInfo.InvariantCulture)));

        // Convert byte array to float array (float values from 4 EMG and 4 IMU sensors)
        ////emgData = new float[(receivedBytes.Length / 5) / floatSize];
        ////Buffer.BlockCopy(receivedBytes, 0, emgData, 0, 4 * floatSize);
        ////imuData = new float[(receivedBytes.Length / 5 * 4) / floatSize];
        ////Buffer.BlockCopy(receivedBytes, 4 * floatSize, imuData, 0, 16 * floatSize);
        //for (int i = 0; i < emgData.Length; ++i)
        //{
        //    Debug.Log("Sensor " + (i + 1).ToString() + ": " + emgData[i].ToString());
        //}
        ////for (int ii = 0; ii < 4; ++ii)
        ////{
        ////    quaternionData[ii] = new Quaternion(imuData[ii * 4 + 1], imuData[ii * 4 + 2], imuData[ii * 4 + 3], imuData[ii * 4 + 4]);
        ////}

        if (!stop)
        {
            Receive(); // <-- this will be our loop
        }
    }

    private void Update()
    {
        imuSensor1.transform.rotation = quaternionData[0];
        imuSensor2.transform.rotation = quaternionData[1];
        imuSensor3.transform.rotation = quaternionData[2];
        imuSensor4.transform.rotation = quaternionData[3];

        //float leftAngle = Quaternion.Angle(quaternionData[0], quaternionData[1]);
        //Debug.Log("Left Angle: " + leftAngle);

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

        if (UIManagerScript.isGameRunning)
        {
            float position = 4 * (deviceAngle - minAngle) / (maxAngle - minAngle);
            userRoot.transform.localScale = new Vector3(userRoot.transform.localScale.x, position, userRoot.transform.localScale.z);
        }

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

    private static float UnwrapAngle(float angle)
    {
        if (angle >= 180)
            return angle -= 360;
        else
            return angle;
    }

    void OnApplicationQuit()
    {
        StopReceiving();
        ////listenerThread.Abort();
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