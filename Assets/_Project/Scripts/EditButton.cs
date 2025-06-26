using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditButton : MonoBehaviour
{
    [SerializeField] private GameObject patientButton;
    [SerializeField] private TMP_Text patientID;

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        Debug.Log($"Edit button of button associated to {patientID.text} has been pressed rightously");
    }

    private void OpenProfileEditor()
    {
        // Opens the editor screen for the patient profile of this button
    }

    public void DoStuff()
    {
        Debug.Log($"Edit Button of {patientID} has been activated");
    }
}
