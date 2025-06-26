using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownScript : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    enum PossibleChoices
    {
        None,
        ChoiceA,
        ChoiceB,
        Choice42
    }

    public Text testChoiceText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetDropDownValue()
    {
        // have to use the indexing of the dropdown
        int chosenEntryIndex = dropdown.value;

        // recover the enum-type value using the index
        PossibleChoices userChoice = (PossibleChoices)Enum.Parse(typeof(PossibleChoices), dropdown.options[chosenEntryIndex].text);
    }

    public void PopulateDropdown()
    {
        // Populates the dropdown yith every possible value of the enum type.
        string[] arrayOfChoices = Enum.GetNames(typeof(PossibleChoices));
        List<string> listOfChoices = new List<string>(arrayOfChoices);
        dropdown.AddOptions(listOfChoices);
    }
}
