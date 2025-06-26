using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using Unity.Collections;
using UnityEngine.UI;
using System;
using System.Linq;

public class TargetMotion : MonoBehaviour
{
    public GameObject target;
    private Vector3 targetStartPosition;
    [SerializeField] private float targetSpeed = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetStartPosition = target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the target position
        target.transform.position = targetStartPosition + new Vector3(0.0f, (-Mathf.Cos(targetSpeed * Time.time) + 1) * 2, 0.0f);

    }

    // experimental stuff to delete
    public int a = 0;
    public int b = 10;
    public int n = 5;
    public List<int> stimSequence;

    public void doStuff()
    {
        GenerateUniqueRandomNumbers(a, b, n);
    }

    public void GenerateUniqueRandomNumbers(int a, int b, int n)
    {
        System.Random rnd = new System.Random();
        stimSequence = Enumerable.Range(a, b - a + 1)
                         .OrderBy(x => rnd.Next())
                         .Take(n)
                         .ToList();
    }
}


