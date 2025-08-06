using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class TrialRunManager : MonoBehaviour
{
    public Slider slider1to10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider1to10.maxValue = 10;
        slider1to10.minValue = 0;
        double stuff = slider1to10.value; 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadSettings()
    {
        SceneManager.LoadScene(2);
    }


    
}


