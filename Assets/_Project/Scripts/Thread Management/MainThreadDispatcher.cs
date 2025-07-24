using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
///  Allows to run actions asked by a secondary thread on the Unity main thread
/// </summary>
public class MainThreadDispatcher : MonoBehaviour
{
    // Queue type containing the action to execute
    // The "Queue" type ensures that actions happen in order
    private static readonly Queue<Action> _toExecuteQueue = new Queue<Action>();

    /// <summary>
    /// Called from the secondary Thread, queue the action/method/etc in the Main Thread, execution at next Update() call.
    /// </summary>
    public static void Enqueue(Action action)
    {
        if (action == null) return;

        // "lock" ensures the code is executed by only one Thread at a time
        // This means that 2 separate secondary threads could be updating it one by one without conflict
        lock (_toExecuteQueue)
        {
            // The "Enqueue" action of the Queue type adds the element at the latest position of the Queue object
            _toExecuteQueue.Enqueue(action);
        }
    }

    // Update is called once per frame
    void Update()
    {
        lock (_toExecuteQueue)
        {
            // While Loop runs until executionQueue is empty
            while (_toExecuteQueue.Count > 0)
            {
                // Dequeue gives the first in line of the Queue AND removes it from the Queue
                var action = _toExecuteQueue.Dequeue();
                action.Invoke();
            }
        }
    }
}

/// Example of Use: 
/// Schedule safe callback on main thread
/// MainThreadDispatcher.Enqueue(() =>
///     {
///         Debug.Log($"Now on main thread: {Thread.CurrentThread.ManagedThreadId}");
///         LJM.Instance.OnLabJackReadingEnd.Invoke(); // Safe here
///     });
//
