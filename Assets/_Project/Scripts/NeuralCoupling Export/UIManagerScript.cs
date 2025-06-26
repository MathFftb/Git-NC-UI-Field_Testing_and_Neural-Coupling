/*
 * Copyright (c) 2018 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerScript : MonoBehaviour 
{
    public Animator startButton;
    public Animator inputButton;
    public Animator inputDialog;
    public Animator exitButton;
    public Button startButtonRef; 
    public static bool isGameRunning;
    public static bool isUserAllowedToMove;
    public GameObject imu1;
    public GameObject imu2;
    public GameObject imu3;
    public GameObject imu4;
    public GameObject visuExitButton;
    public GameObject target;
    public GameObject targetVisualization;
    public GameObject playButton;

    public Text movCycleSliderLabel;
    public Slider movCycleSlider;
    public Text stimAmountSliderLabel;
    public Slider stimAmountSlider;
    public Text movSpeedSliderLabel;
    public Slider moveSpeedSlider;
    public Text thresholdSliderLabel;
    public Slider thresholdSlider;

    private Vector3 _startPosition;

    private bool isCooperative = true;
    private int movementCycles = 30;
    private int stimulationCycles = 20;
    private float speed = 2f;
    public static int stimThreshold = 20;

    private float timeCounter;
    public float timeTillNextCycle;
    private float periodTillNextCycle;
    public int currMovementCycle;
    public List<int> stimSequence;

    public static bool isStimTrial = false;
    public static bool isDummyTrial = false;
    public static bool hasStimBeenGiven = false;
    public static bool hasDummyBeenGiven = false;

    public static bool shouldDummyBeRecorded = false;

    public GameObject wavePlusManager;
    private int trialNumber = 0;

    private void Awake()
    {
        // Set game to paused
        isGameRunning = PauseGame();
        // Do not let user move
        isUserAllowedToMove = false;
        visuExitButton.SetActive(false);
        _startPosition = target.transform.position;

        // Hide targets
        targetVisualization.SetActive(false);
        timeCounter = 0f;
        currMovementCycle = 1;
        playButton.SetActive(false);
    }

    private void GetTimeTillNextCycle()
    {
        periodTillNextCycle = 2 * Mathf.PI / speed;
        timeTillNextCycle = periodTillNextCycle;
    }

    public void CreateStimSequence()
    {
        List<int> randomSequence = new List<int>(movementCycles);
        for (int ii = 1; ii <= movementCycles; ii++)
        {
            randomSequence.Add(ii);
        }
        randomSequence = Shuffle(randomSequence);

        stimSequence = new List<int>(stimulationCycles);
        for (int jj = 1; jj <= stimulationCycles; jj++)
        {
            stimSequence.Add(randomSequence[jj]);
        }
        stimSequence.Sort();
    }

    public static List<int> Shuffle(List<int> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
        return ts;
    }

    public void ChangeMode(Toggle cooperative)
    {
        // If mode is cooperative
        if(cooperative)
        {
            isCooperative = true;
        }
        else
        {
            isCooperative = false;
        }
    }

    public void ChangeAmountMovementCycles(Slider movementSlider)
    {
        // Store user selected number of movement cycles and update label
        movementCycles = (int)movementSlider.value;
        movCycleSliderLabel.text = movementCycles.ToString();

        // If more movement than stimulation cycles are selected
        if (movementCycles < stimulationCycles)
        {
            // Equal number of movement and stimulation cycles
            stimulationCycles = movementCycles;
            // Update stimulation cycle slider and label
            stimAmountSlider.value = stimulationCycles;
            stimAmountSliderLabel.text = stimulationCycles.ToString();
        }
    }

    public void ChangeAmountStimCycles(Slider stimSlider)
    {
        // Store user selected number of stimulation cycles and update label
        stimulationCycles = (int)stimSlider.value;
        stimAmountSliderLabel.text = stimulationCycles.ToString();

        // If more stimulation than movement cycles are selected
        if (movementCycles < stimulationCycles)
        {
            // Equal number of movement and stimulation cycles
            movementCycles = stimulationCycles;
            // Update movement cycle slider and label
            movCycleSlider.value = movementCycles;
            movCycleSliderLabel.text = movementCycles.ToString();
        }
    }

    public void ChangeMovementSpeed(Slider speedSlider)
    {
        // Store user selected movement speed and update label
        speed = speedSlider.value;
        movSpeedSliderLabel.text = speed.ToString();
    }

    public void VisualizeSensors()
    {
        startButton.SetBool("isHidden", true);
        inputButton.SetBool("isHidden", true);
        inputDialog.SetBool("isHidden", true);
        exitButton.SetBool("isHidden", true);
        imu1.GetComponentInChildren<Renderer>().enabled = true;
        imu2.GetComponentInChildren<Renderer>().enabled = true;
        imu3.GetComponentInChildren<Renderer>().enabled = true;
        imu4.GetComponentInChildren<Renderer>().enabled = true;
        visuExitButton.SetActive(true);
    }

    public void ExitVisualization()
    {
        visuExitButton.SetActive(false);
        imu1.GetComponentInChildren<Renderer>().enabled = false;
        imu2.GetComponentInChildren<Renderer>().enabled = false;
        imu3.GetComponentInChildren<Renderer>().enabled = false;
        imu4.GetComponentInChildren<Renderer>().enabled = false;
        startButton.SetBool("isHidden", true);
        inputButton.SetBool("isHidden", true);
        inputDialog.SetBool("isHidden", false);
        exitButton.SetBool("isHidden", true);
    }

    public void ChangeThreshold(Slider threshSlider)
    {
        // Store user selected stimulation threshold and update label
        stimThreshold = (int)threshSlider.value;
        thresholdSliderLabel.text = stimThreshold.ToString() + "%";
    }

    public void StartGame() 
    {
        startButton.SetBool("isHidden", true);
        inputButton.SetBool("isHidden", true);
        inputDialog.SetBool("isHidden", true);
        exitButton.SetBool("isHidden", true);
        StartCoroutine("FadeToGame");
    }

    public void CloseSettings()
    {
        startButton.SetBool("isHidden", false);
        inputButton.SetBool("isHidden", false);
        inputDialog.SetBool("isHidden", true);
        exitButton.SetBool("isHidden", false);
    }

    public void OpenInputSettings()
    {
        startButton.SetBool("isHidden", true);
        inputButton.SetBool("isHidden", true);
        inputDialog.SetBool("isHidden", false);
        exitButton.SetBool("isHidden", true);
    }


    public static bool PauseGame()
    {
        Time.timeScale = 0;
        return false;
    }

    public static bool ResumeGame()
    {
        Time.timeScale = 1;
        return true;
    }

    IEnumerator FadeToGame()
    {
        // Reset trial parameters
        currMovementCycle = 1;
        timeCounter = 0f;
        GetTimeTillNextCycle();
        CreateStimSequence();
        isStimTrial = false;
        isDummyTrial = false;
        hasStimBeenGiven = false;
        hasDummyBeenGiven = false;
        shouldDummyBeRecorded = false;
        trialNumber += 1;
        playButton.SetActive(true);
        

        // Wait for 1.5 seconds
        yield return new WaitForSecondsRealtime(1.5f);
        
        // Activate target
        targetVisualization.SetActive(true);
        isUserAllowedToMove = true;
    }

    public void StartTrialButton()
    {
        StartCoroutine("StartTrial");

    }

    IEnumerator StartTrial()
    {
        // Hide play button
        playButton.SetActive(false);

        // Wait for 1.5 seconds
        yield return new WaitForSecondsRealtime(1.5f);

        isGameRunning = true;

        // Resume game
        ResumeGame();
    }

    private void Update()
    {
        if (isGameRunning)
        {
            timeCounter += Time.deltaTime;
            if (timeCounter > timeTillNextCycle)
            {
                currMovementCycle += 1;
                timeTillNextCycle += periodTillNextCycle;
                // Reset stimulation flag
                hasStimBeenGiven = false;
                hasDummyBeenGiven = false;
            }

            // Check if stimulation should be given during current cycle 
            if (stimSequence.Contains(currMovementCycle) && !hasStimBeenGiven)
            {
                isStimTrial = true;
            }

            // Check if dummy should be given during current cycle 
            if (!stimSequence.Contains(currMovementCycle) && !hasDummyBeenGiven)
            {
                isDummyTrial = true;
            }

            // Update target position
            target.transform.position = _startPosition + new Vector3(0.0f, (-Mathf.Cos(speed * Time.time) + 1) * 2, 0.0f);

            // If user cancels trial or number of movment cycles have passed
            if (Input.GetKeyDown(KeyCode.Escape) || currMovementCycle > movementCycles)
            {
                // End trial
                isGameRunning = PauseGame();
                isUserAllowedToMove = false;
                // Save data
                wavePlusManager.GetComponent<WaveplusDaqScript>().SaveDataAsync(trialNumber);

                // Show next trial menu
                startButtonRef.GetComponentInChildren<Text>().text = "Next Trial";
                startButton.SetBool("isHidden", false);
                inputButton.SetBool("isHidden", false);
                targetVisualization.SetActive(false);
                target.transform.position = _startPosition;
            }
        }
    }

    public void SaveSettingsAsync(string filename)
    {
        // Open settings file to save trial settings
        using (var outf = new StreamWriter("C:\\Users\\Mathieu\\Downloads\\" + filename))
        {
            // Write settings to file
            outf.WriteLine("# Movement Cycles:");
            outf.WriteLine(movementCycles.ToString());  //Number of movement cycles
            outf.WriteLine("");
            outf.WriteLine("# Stimulations:");
            outf.WriteLine(stimulationCycles.ToString());   //Number of stimulations
            outf.WriteLine("");
            outf.WriteLine("Target Speed:");
            outf.WriteLine(speed.ToString());       //Speed of target
            outf.WriteLine("");
            outf.WriteLine("Stimulation threshold:");
            outf.WriteLine(stimThreshold.ToString());   //Stimulation threshold as percentage of ROM
            outf.WriteLine("");
            outf.WriteLine("Stimulation sequence:");
            foreach(int seqString in stimSequence)
            {
                outf.Write(seqString.ToString() + ", ");    //List of stimulation trial indices
            }
        }
    }

    public void ExitGame()
    {
#if (UNITY_EDITOR)
        {
            UnityEditor.EditorApplication.ExitPlaymode();
        }
#else
        {
            Application.Quit();
        }
#endif
    }
}
