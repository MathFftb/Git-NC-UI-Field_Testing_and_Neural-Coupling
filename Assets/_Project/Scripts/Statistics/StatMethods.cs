using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using System.Drawing;



// Just some useful methods to get some stats on data
public class StatMethods : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetQuartileValues(LabJackObject.LabJackDataPoint[] labJackArray, out double q1, out double q2, out double q3)
    {
        List<double> listToSort = new();

        // for (int i = 0; i < labJackArray.Length; ++i)
        // {
        //     listToSort.Append(labJackArray[i].AIN0); // Value of interest here is the AIN0
        // }

        listToSort = labJackArray.Select(datapoint =>datapoint.AIN0).ToList();// Value of interest here is the AIN0
        listToSort.Sort();
        
        q1 = listToSort[(int)Math.Round((double)(listToSort.Count / 4))];
        q2 = listToSort[(int)Math.Round((double)(listToSort.Count / 2))];
        q3 = listToSort[(int)Math.Round((double)(listToSort.Count *3 / 4))];
    }

    public void GetQuartileValues<T>(in T[] labJackArray, out T q1, out T q2, out T q3)
    {
        T[] arrayToSort = (T[])labJackArray.Clone();
        Array.Sort(arrayToSort); // Need to have a functioning CompareTo method for the T 

        q1 = arrayToSort[(int)Math.Round((double)(arrayToSort.Length / 4))];
        q2 = arrayToSort[(int)Math.Round((double)(arrayToSort.Length / 2))];
        q3 = arrayToSort[(int)Math.Round((double)(arrayToSort.Length *3 / 4))];
    }

    public void GetQuartileValue<T>(in T[] labJackArray, decimal fraction, out T q)
    {
        T[] arrayToSort = (T[])labJackArray.Clone();
        Array.Sort(arrayToSort); // Need to have a functioning CompareTo method for the T 

        q = arrayToSort[(int)Math.Round((double)(arrayToSort.Length * fraction))];
    }

    public T[] WithoutOutliers<T>(in T[] dataArray, decimal qmin, decimal qmax)
    {
        T[] arrayToSort = (T[])dataArray.Clone();

        Array.Sort(arrayToSort);


        int lengthCleaned = (int)Math.Round((decimal)(1 - qmin - qmax));
        T[] cleanedArray = new T[lengthCleaned];
        int arrayStart = (int)Math.Round((decimal)(arrayToSort.Length * qmin));
        int arrayStop = (int)Math.Round((decimal)(arrayToSort.Length * qmax));
        for (int i = arrayStart; i < arrayStop; ++i)
        {
            cleanedArray[i] = arrayToSort[i];
        }
        // All of this results in an array sorted by value: dates are out of order

        // With List

        // Get the cutoff values
        T qminVal, qmaxVal;
        T[] array = (T[])dataArray.Clone();
        Array.Sort(arrayToSort); // Need to have a functioning CompareTo method for the T 

        qminVal = array[(int)Math.Round((double)(arrayToSort.Length * qmin))];
        qmaxVal = array[(int)Math.Round((double)(arrayToSort.Length * qmax))];

        List<T> cleanedList = dataArray.ToList();

        return cleanedArray;
    }

    public void TrimDataArrayUsingIndex<T>(T[] data, decimal qmin, decimal qmax) where T : IComparable<T>
    {
        // Create an index fitting the array
        int[] sortedIndices = Enumerable.Range(0, data.Length).ToArray();

        // Sort the indices by the datapoint attributed to each index
        Array.Sort(sortedIndices, (i, j) => data[i].CompareTo(data[j])); // This supposes an existing CompareTo method for the datapoints

        // Define which fraction of the index to keep
        int cleanedStart = (int)Math.Round(data.Length * qmin);
        int cleanedStop = (int)Math.Round(data.Length * qmax);

        // List<T> cleanedList = new();
        // for (int i = cleanedStart; i < cleanedStop; ++i)
        // {
        //     // Fill the list with a fixed number of points, leaving out the 
        //     cleanedList.Append(data[
        //                         sortedIndices[i]
        //                         ]);
        // }

        // //Create a list object to be able to pop undesired outliers out of it
        // List<T> cleanedList = data.ToList();
        // for (int i = 0; i < cleanedStart; ++i)
        // {
        //     // Remove the first outliers
        //     cleanedList.RemoveAt(sortedIndices[i]);
        // }

        //Remove the index of the outliers in the sorted index list
        List<int> trimmedIndices = new List<int>(sortedIndices);
        // Defensive checks 
        cleanedStart = Math.Clamp(cleanedStart, 0, sortedIndices.Length);
        cleanedStop  = Math.Clamp(cleanedStop, 0, sortedIndices.Length);

        // Remove the end outliers
        trimmedIndices.RemoveRange(cleanedStop, trimmedIndices.Count - cleanedStop);
        // Remove the bottom outliers
        trimmedIndices.RemoveRange(0, cleanedStart);
        // Sort by the value of the index: put them back in original order
        trimmedIndices.Sort();

        int cleanedLength = trimmedIndices.Count;
        T[]cleanedArray = new T[cleanedLength];
        for (int i = 0; i < cleanedLength; ++i)
        {
            cleanedArray[i] = data[
                                    trimmedIndices[i]
                                    ];
        }

        // Sort the index list by index, so that it recovers the order it was in before

        // Create an array to null the outlier datapoints
        // T[] cleanedArray = (T[])data.Clone();
        // // create an int to remember the expected size of the final array and prevent an error
        // int cleanedLength = 0; 

        // for (int i = 0; i < cleanedStart; ++i)
        // {
        //     cleanedArray[i] = null;
        // }

        //Create an array to be a full cleaned copy of data
        //List<T> cleanedList = new();
    }

    // Auxiliary method that determines how to sort two different datapoints described as in the LabJackObject.cs struct
    // Returns -1 if datapoint2 is bigger, 1 if datapoint1 is bigger, 0 if they are equal.
    public static int LJMDataComparisonFunction(LabJackObject.LabJackDataPoint datapoint1, LabJackObject.LabJackDataPoint datapoint2)
    {
        var value1 = datapoint1.AIN0;
        var value2 = datapoint2.AIN0;

        return value1.CompareTo(value2);
    }
}
