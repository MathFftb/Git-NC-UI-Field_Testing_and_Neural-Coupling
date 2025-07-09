using System;
using System.Data;
using System.Data.Common;
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

    public static string ToCsv<T>(T[] dataSet, Func<T, string> toCsvLine, string header = null)
    {
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(header))
            sb.AppendLine(header);

        foreach (T dataPoint in dataSet)
        {
            sb.AppendLine(toCsvLine(dataPoint));
        }

        return sb.ToString();
    }

    public static string ToCsv(double dp)
    {
        return dp.ToString();
    }

    
}
