using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

public class CsvConverter : MonoBehaviour
{
    struct DataPoint
    {
        public double AIN0;

        public override string ToString()
        {
            return $"AIN0 = {AIN0}\n";
        }

        public string ToCsv()
        {
            return $"{AIN0},";
        }
        public static string CsvHeader()
        {
            return "AIN0,AIN1,Timestamp";
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        double[] ds = new double[10];
        ToCsv(ds, db => ToCsv(db));

        DataPoint[] ds2 = new DataPoint[10];
        ToCsv(ds2, dp => dp.ToCsv(), DataPoint.CsvHeader());
    }

    private void justCompileTest()
    {
        double[] ds = new double[10];
        ToCsv(ds, db => ToCsv(db));

        DataPoint[] ds2 = new DataPoint[10];
        ToCsv(ds2, dp => dp.ToCsv(), DataPoint.CsvHeader());
    }

    public static string ToCsv<T>(T[] dataset, Func<T, string> toCsvLine, string header = null)
    {
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(header))
            sb.AppendLine(header);

        foreach (T dataPoint in dataset)
        {
            sb.AppendLine(toCsvLine(dataPoint));
        }

        return sb.ToString();
    }

    public static string ToCsv(double dp)
    {
        return dp.ToString();
    }


    // Segment the process to avoid memory issues in Unity when passing by a string
    public static void SaveAsCsv<T>(T[] dataset, Func<T, string> toCsvLine, string saveFilePath, string header = null)
    {
        // Opens the file where the csv si going to be saved, or creates it if it did not exist
        // The "using" statement automatically calls Dispose on the object when the code that is using it has completed.
        using (StreamWriter writer = new StreamWriter(saveFilePath))
        {
            // Write Header if given
            if (!string.IsNullOrEmpty(header))
                writer.WriteLine(header);

            // Save each datapoint as a csv encoded line
            foreach (T dataPoint in dataset)
            {
                writer.WriteLine(toCsvLine(dataPoint));
            }
        }
        Debug.Log($"Csv save created at {saveFilePath}");
        // Note: this method would only append a pre-existing file and not overwrite it

    }

    
}
