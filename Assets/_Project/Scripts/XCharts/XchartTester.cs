using System;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using XCharts.Runtime;


public class XchartTester : MonoBehaviour
{
    public LineChart chart; // Assign via inspector or GetComponent
    private float timeCounter = 0f;
    private int maxDataPoints = 100;

    private Serie serie;
    public int serieMaxCache;
    public float serieLineWidth = 3f;
    public bool serieStyleIsArea = false;

    public int userInt = 0;
    public TMP_InputField userIntEntryField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chart.EnsureChartComponent<Title>().text = "Real-Time Torque Data";
        chart.ClearData();

        chart.AddSerie<Line>("Torque");

        // Optional: Customize appearance
        chart.GetSerie(0).symbol.show = false;

        serie = chart.GetSerie(0);
        serie.maxCache = serieMaxCache;
        serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin
    }

    // Update is called once per frame
    void Update()
    {
        //AddRandomPoint();
        timeCounter += Time.deltaTime;
    }

    public void AddRandomPoint()
    {
        float value = UnityEngine.Random.Range(1, 10);
        AddTorqueDataPoint(value);
        Debug.Log($"Added value {value} to graph.");
    }

    public void AddRandomPointArray(int numberAdded)
    {
        for (int i = 0; i < numberAdded; i++)
        {
            AddRandomPoint();
        }
    }
    public void AddRandomPointArray()
    {

        for (int i = 0; i < userInt; i++)
        {
            serie.AddData(i, UnityEngine.Random.Range(0f, 10f));
        }
    }

    public void SimulateSpikeAmongPoints()
    {
        float Tcurrent = 0;
        for (int i = 0; i < userInt * 0.45; ++i)
        {
            serie.AddData(i, UnityEngine.Random.Range(4f, 6f));
            Tcurrent = i;
        }

        for (int i = (int)(userInt * 0.45); i < userInt * 0.55; ++i)
        {
            float aValue = UnityEngine.Random.Range(4f, 6f) + EMGSpike(i, Tcurrent + 20);
            serie.AddData(i, aValue);
        }
        for (int i = (int)(userInt * 0.55); i < userInt; ++i)
        {
            serie.AddData(i, UnityEngine.Random.Range(4f, 6f));
        }
    }

    public float EMGSpike(float t, float t0, float A = 4f, float riseTime = 10f, float decayTime = 50f)
    {
        if (t < t0) return 0f;

        float dt = t - t0;

        if (dt <= riseTime)
        {
            // Linear rise
            return A * (dt / riseTime);
        }
        else if (dt <= riseTime + decayTime)
        {
            // Linear decay
            return A * (1 - (dt - riseTime) / decayTime);
        }
        else
        {
            return 0f;
        }
    }

    public void OnUserIntChanged()
    {
        if (userIntEntryField.text == "")
            userInt = 0;
        else
        {
            userInt = int.Parse(userIntEntryField.text);
        }
    }

    public void AddTorqueDataPoint(float torque)
    {
        // Add new data
        serie.AddData(timeCounter, torque);

        // Keep data length manageable
        if (serie.dataCount > maxDataPoints)
        {
            serie.RemoveData(0);
        }

        chart.RefreshChart(); // Force refresh
    }

    public void ClearGraph()
    {
        chart.ClearData();
    }

    [ContextMenu("Update Max Cache Value")]
    public void UpdateMaxCacheValue()
    {
        serie.maxCache = serieMaxCache;
        chart.RefreshChart();
    }

    [ContextMenu("Update Line Width")]
    public void UpdateLineWidth()
    {
        serie.lineStyle.width = serieLineWidth;  // Default is usually 2 or 3; set to 1 for thin
        chart.RefreshChart();
    }

    /*//Bugged, maybe you cannot change the style once the chart is built?
    [ContextMenu("Toggle Area Chart Style")]
    public void ToggleAreaChartStyle()
    {
        //     // Enable area style (this is what creates the fill under the line)
        //     if (serie.areaStyle == null)
        //     {
        //         serie.areaStyle. = new AreaStyle();
        //     }

        //     if (serieStyleIsArea)
        //             serie.areaStyle.show = false;
        //         else
        //         {
        //             serie.areaStyle.show = true;
        //         }
        //     // Optional: Set the fill color
        //     serie.areaStyle.color = new Color(0.2f, 0.6f, 1f, 0.3f);  // Semi-transparent blue
        //     chart.RefreshChart();
        //
        // var areaStyle = serie.areaStyle;
        // areaStyle.show = true;
        // areaStyle.color = new Color(0.2f, 0.6f, 1f, 0.3f); // semi-transparent blue 
        var line = chart.AddSerie<Line>("Torque");

        // This will now have areaStyle initialized internally
        line.lineType = LineType.Smooth;
        line.areaStyle.show = true;
    }*/
    
}
