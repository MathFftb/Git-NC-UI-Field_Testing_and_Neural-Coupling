using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public int sensorIndex;
    public int windowSize;
    public bool isRectified;
    public bool isSmoothed;
    public Dropdown sensorSelection;
    public InputField windowInputField;
    public Toggle rectifiedToggle;
    public Toggle smoothedToggle;
    

    // Start is called before the first frame update
    void Awake()
    {   
        // Initialize starting values
        sensorIndex = 1;
        windowInputField.text = "100";
        windowSize = 100;
        isRectified = rectifiedToggle.isOn;

        // Add listener
        sensorSelection.onValueChanged.AddListener(delegate { SensorSelectionChangedCheck(); });
        windowInputField.onEndEdit.AddListener(delegate { WindowSizeChangedCheck(); });
        rectifiedToggle.onValueChanged.AddListener(delegate { RectifiedToggleChangedCheck(); });
        smoothedToggle.onValueChanged.AddListener(delegate { SmoothedToggleChangedCheck(); });

        // Only allow integer input
        windowInputField.characterValidation = InputField.CharacterValidation.Integer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void WindowSizeChangedCheck()
    {
        int.TryParse(windowInputField.text, out windowSize);
        if(windowSize < 0)
        {
            windowSize = Mathf.Abs(windowSize);
            windowInputField.text = windowSize.ToString();
        }
    }

    private void RectifiedToggleChangedCheck()
    {
        isRectified = rectifiedToggle.isOn;
    }

    private void SmoothedToggleChangedCheck()
    {
        isSmoothed = smoothedToggle.isOn;
    }

    public void SensorSelectionChangedCheck()
    {
        // Get selected sensor index
        sensorIndex = sensorSelection.value + 1; //value: zero-based index
    }
}
