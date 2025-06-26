using System.Collections.Generic;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEditor.SearchService;
using UnityEngine;


public class PatientListManager : MonoBehaviour
{
    [SerializeField]
    private GameObject patientProfileTemplate;

    [SerializeField]
    private GameObject templateParent;

    [SerializeField]
    private bool automateFindParent = false;

    // This array is set in the inspector, it could be loaded with existing data
    // (e.g: patient profile info list)
    [SerializeField]
    private int[] intArray;

    [SerializeField]
    private PatientInfo[] patients;

    private List<GameObject> patientButtonList;

    //Custom Types
    struct PatientInfo
    {
        string ID;
    }

    void Start()
    {
        CreateButtonList();
    }

    void CreateButtonList()
    {
        patientButtonList = new List<GameObject>();

        // Prevents duplication of buttons
        if (patientButtonList.Count > 0)
        {
            foreach (GameObject button in patientButtonList)
            {
                Destroy(button.gameObject);
            }
        }

        foreach (int i in intArray)
        {
            GameObject button = Instantiate(patientProfileTemplate) as GameObject;
            button.SetActive(true);

            button.GetComponent<ButtonListButton>().SetText("Button #" + i + "Just a radom corpus to test the limit of the scrollable space, if it goes this far then it is more than good.");

            if (automateFindParent) button.transform.SetParent(patientProfileTemplate.transform.parent, worldPositionStays: false);
            else button.transform.SetParent(templateParent.transform, worldPositionStays: false);
        }
    }

    public void ButtonClicked(string textString)
    {
        Debug.Log(textString);
    }



    void GenButtons()
    {

    }
}
