using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public PatientProfile getProfile()
    {
        return patientProfile;
    }

    public void SetPatientProfile(PatientProfile newProfile)
    {
        this.patientProfile.CopyFrom(newProfile);
    }

    // Old Code starts here 
    // The Overseer is the vehicle of information across scenes
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

        LoadBestScore();
    }

    // Persistence Implementation

    // Scene Persistence Implementation

    

    // Session Persistence Implementation

    /* This BestScore Variable exists separately from the one in the SaveData
        But they represent the same thing, should I just use SaveData?
        I think it is safer to use SaveData only in cases of information passation
        and manipulate the direct BestScore during the run
    */

    public int BestScore;
    public string BestPlayer;

    public string Player;

    // SaveData component of Overseer
    // Contains everything that needs to survive across sessions
    [SerializeField]
    class SaveData
    {
        //not sure about the private or public declarations in these parts
        // Internet got me confused and idk if I should set up get and set for BestScore
        public int BestScore;
        public string BestPlayer;
        // for when we will want to add a highscore screen displaying a list of the top scores and players
        // public (string Player, int Score)[] BestScores;
    }
    
    // Save files' paths and names
    string saveFileName = "savefile.json";
    string saveFilePath;

    public void InitializeFilePathAndName()
    {
        saveFilePath = Application.persistentDataPath + "/" + saveFileName;
    }

    public void UpdateBestScore(int newScore)
    {
        if(newScore>BestScore)
        {
            BestScore = newScore;
            BestPlayer = Player;
        }
    }
    
    public void SaveBestScore()
    {
        SaveData recentSave = new SaveData();
        recentSave.BestScore = BestScore;
        recentSave.BestPlayer = BestPlayer;

        string jsonFormatedSave = JsonUtility.ToJson(recentSave);

        File.WriteAllText(saveFilePath , jsonFormatedSave);
    }

    public void LoadBestScore()
    {
        // goes to the supposed existing save file
        string path = saveFilePath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            BestScore = data.BestScore;
            BestPlayer = data.BestPlayer;
        }
        // If there is no save the best score is 0
        else
        {
            BestScore = 42;
        }
    }
}
