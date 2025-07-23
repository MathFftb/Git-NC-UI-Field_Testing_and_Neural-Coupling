using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.Remoting.Messaging;

public class Overseer : MonoBehaviour
{
    /* Rules:
    This class contains all the data necessary to pass between screen 
    that define the current trial:
    From Patient Selector:
    -patient selected:
        -ID 
        -All other data(?)
    
    From Trial Setting:
    -Trial settings:
        -Nb of cycles
        -Nb of stimulations
        -Type of experiment
        -Side
        -Stimulation Intensity
        -Fixed or Variable Torque

    Exiting Profile Selection:
    -Profile has been selected
    Entering Trial Setting:
    -MVC data exists

    Exiting Trial Settings:
    -Cooperative/Not and Side have been selected
    -MVC data has been selected
    -Stimulation intensity has been selected

    */

    [SerializeField] private PatientProfile patientProfile;

    public LabJackObject LJM;

    [SerializeField]
    public double MVCValue { get; set; }


    public static Overseer Instance;


    private void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFilePathAndName();

        LJM.InitializeAllValues();

        Debug.Log("Called Awake Overseer");
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //LabJack 
        // LabJack
        if (LJM.timerIsRunning)
        {
            LJM.timeReadingSec += Time.deltaTime;
            if (LJM.timeReadingSec > LJM.maxTimeReadLoopSec)
                LJM.timerIsRunning = false;
        }
    }

    public PatientProfile getProfile()
    {
        return patientProfile;
    }

    public void SetPatientProfile(PatientProfile newProfile)
    {
        this.patientProfile.CopyFrom(newProfile);
    }


    #region Path Management
    [Header("Paths")]
    public string projectPath; // Where the project runs
    public string dataFolderPath; // Where all data is stored

    public string patientFolderPath; // Where the current patient's info and sessions are stored
    public string patientProfileInfoFilePath; // Where the current PatientProfile.json is stored

    public string sessionsFolderPath; // Where all the current patient's sessions data is stored
    public string currentSessionFolderPath; // Where the current session is stored

    public string MVCMeasurementsFolderPath; // Where the 3 MVC Measurements AND the User determined MVC Value are stored
    public string currentMVCMeasurementFilePath; // Where the current one of the 3 MVC Measurement data is stored
    public string MVCValueFilePath; // Where the User-Determined MVC Value is stored

    public string currentTrialFilePath; // Where the current trial data is stored

    [Header("File Names")] // What we call the files in the data structure, some are generic others have to be changed during run
    public string dataFolderName = "Data";

    public string patientFolderName = "Patient ID";
    public string patientProfileInfoFileName = $"PatientProfileInfo.json";

    public string sessionFolderName = "Sessions";
    public string currentSessionName = $"Session X";

    public string MVCMeasurementsFolderName = "MVC";
    public string currentMVCMeasurementFileName = $"MVC Measurement X";
    public string MVCValueFileName = "MVC Value";

    public string currentTrialFileName = $"Trial X";


    public void InitializeFilePathAndName()
    {
        // // Initialize all files and folder names
        // patientFolderName = "Patient ID";
        // patientProfileInfoFileName = $"PatientProfileInfo.json";

        // sessionFolderName = "Sessions";
        // currentSessionName = $"Session X";

        // MVCMeasurementsFolderName = "MVC"; 
        // currentMVCMeasurementFileName = $"MVC Measurement X";
        // MVCValueFileName = "MVC Value";

        // currentTrialFileName = $"Trial X";

        UpdateAllPaths();
    }

    public void UpdateAllPaths()
    {
        // Initialize Root Project Path
        projectPath = new DirectoryInfo(Application.dataPath).Parent.FullName;   //dataPath is the path to the Assets folder of the project in Unity
                                                                                 //path is now a string containing the path to the parent Folder of Assets:= the Project Folder
                                                                                 // Initialize Data Folder's path
        dataFolderPath = projectPath + "\\" + dataFolderName;

        // Initialize all other paths
        patientFolderPath = $"{dataFolderPath}\\{patientFolderName}";

        patientProfileInfoFilePath = $"{patientFolderPath}\\{patientProfileInfoFileName}";

        sessionsFolderPath = patientFolderPath + "\\" + sessionFolderName;
        currentSessionFolderPath = sessionsFolderPath + "\\" + currentSessionName;

        MVCMeasurementsFolderPath = patientFolderPath + "\\" + MVCMeasurementsFolderName;
        currentMVCMeasurementFilePath = MVCMeasurementsFolderPath + "\\" + currentMVCMeasurementFileName;
        MVCValueFilePath = MVCMeasurementsFolderPath + "\\" + MVCValueFileName;

        Debug.Log("Paths Updated");
    }

    #endregion




}
