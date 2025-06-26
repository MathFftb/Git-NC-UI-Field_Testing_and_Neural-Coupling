using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class ButtonListControl : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonTemplate;

    // This array is set in the inspector, it could be loaded with existing data
    // (e.g: patient profile info list)
    [SerializeField]
    private int[] intArray;

    private List<GameObject> buttonsList;

    void Start()
    {
        CreateButtons();
    } 

    void CreateButtons()
    {
        buttonsList = new List<GameObject>();

        // Prevents duplication of buttons
        if (buttonsList.Count > 0)
        {
            foreach (GameObject button in buttonsList)
            {
                Destroy(button.gameObject);
            }
        }

        foreach (int i in intArray)
            {
                GameObject button = Instantiate(buttonTemplate) as GameObject;
                button.SetActive(true);

                button.GetComponent<ButtonListButton>().SetText("Button #" + i + "Just a radom corpus to test the limit of the scrollable space, if it goes this far then it is more than good.");

                button.transform.SetParent(buttonTemplate.transform.parent, worldPositionStays: false);
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
