using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Unity.Collections;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.Events;

public class TargetMotion : MonoBehaviour
{
    [SerializeField] TrialState currentState;

    public enum TrialState
    {
        Starting,
        RunningExperiment,
        Tutorial,
        Ending
    }

    [SerializeField] double elapsedTime = 0;
    [SerializeField] int cyclesMax;
    [SerializeField] int currentCycle;
    [SerializeField] double cycleFrequency;
    [SerializeField] double cyclePeriod;


    [SerializeField] double maxYPosition;
    [SerializeField] double maxTargetMovementRange = 2; 
    
    public GameObject targetObject;
    private Vector3 targetStartPosition;
    [SerializeField] private float targetSpeed = 2f;

    public GameObject playerObject;

    [SerializeField] bool userStopsExperiment= false;

    public UnityEvent OnGameEnd; 


    void Start()
    {
        currentState = TrialState.Starting;
        targetStartPosition = targetObject.transform.position;
    }

    void Update()
    {
        
    }

    // Coroutine launched when the trial starts
    [ContextMenu("Run Cycles")]
    public void StartExperiment()
    {
        StartCoroutine(RunningExperiment());
    }
    public IEnumerator RunningExperiment()
    {
        // Change gamestate
        currentState = TrialState.RunningExperiment;

        // Initiate Variables
        //cyclePeriod = maxTargetMovementRange * Mathf.PI / targetSpeed;
        targetSpeed = (float) (Math.PI * maxTargetMovementRange / cyclePeriod); 

        currentCycle = 0;
        elapsedTime = 0;
        double timeOfNextCycle = cyclePeriod;
        float radius = (float)maxTargetMovementRange / 2;
        // Run all the cycles
        while ((currentCycle < cyclesMax) && (!userStopsExperiment))
        {
            // Update target position
            targetObject.transform.position = targetStartPosition + new Vector3(0.0f, (+Mathf.Sin(targetSpeed * (float)elapsedTime / radius)) * radius, 0.0f);


            // if the loops has entered a new cycle
            if (elapsedTime > timeOfNextCycle)
            {
                // Update the mark for the future next cycle
                timeOfNextCycle += cyclePeriod;
                // update cycle count
                ++currentCycle;
            }

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        // When exiting the loop, the game has ended
        OnGameEnd.Invoke();
    }

    

    #region "Generate Stimulation sequence"

    // experimental stuff to delete
    public int a = 0;
    public int b = 10;
    public int n = 5;
    public List<int> stimSequence;

    public void GenerateUniqueRandomNumbers(int a, int b, int n)
    {
        System.Random rnd = new System.Random();
        stimSequence = Enumerable.Range(a, b - a + 1)
                         .OrderBy(x => rnd.Next())
                         .Take(n)
                         .ToList();
    }
    #endregion
}


