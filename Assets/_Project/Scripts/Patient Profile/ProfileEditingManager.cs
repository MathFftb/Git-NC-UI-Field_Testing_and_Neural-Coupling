using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System.Net.NetworkInformation;
using UnityEngine.Assertions.Must;
using System.Diagnostics.Eventing.Reader;

public class ProfileEditingManager : MonoBehaviour
{

    [Header("New Proposed Profile Info")]
    public string ID;
    public int age;
    public PatientProfile.StudyCategory category;
    public DateTime timeOfStroke;
    public PatientProfile.Side affectedSide;
    public PatientProfile.Side handedness;

    [Header("User Input Fields")]
    public TMP_InputField IDField;
    public TMP_InputField ageField;
    public TMP_Dropdown categoryDropdown; // has to somehow become a dropdown menu
    public DateSelector timeOfStrokeChoice; // need to change to reflect selection of formatted date
    public TMP_Dropdown affectedSideDropdown; // has to be a binary choice
    public TMP_Dropdown handednessChoice; // has to be a binary choice

    [Header("Category-Dependent UI Elements")]
    public Button timeOfStrokeEditor;
    public Button affectedSideEditor;

    [Header("Edited Profile")]
    [SerializeField]
    private PatientProfile editedProfile;

    //private bool isIdValid;
    //private bool isAgeValid;
    private bool isCategoryValid;
    private bool isStrokeDateValid;
    private bool isStrokeDateFormatValid;
    private bool isAffectedSideValid;
    //private bool isHandednessValid;


    [Header("Buttons")]
    public Button confirmChangesButton;

    [Header("Error Windows")]
    public TextMeshProUGUI errorWindow;

    [Serializable]
    public class DateSelector
    {
        public TMP_Dropdown DayChoice;
        public TMP_Dropdown MonthChoice;
        public TMP_Dropdown YearChoice;
 
        public IEnumerable<TMP_Dropdown> AllElements
        {
            get
            {
                yield return DayChoice;
                yield return MonthChoice;
                yield return YearChoice;
            }
        }
    }

    #region Main

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        errorWindow.text = "";
        //editedProfile = Overseer.Instance.getProfile();
        editedProfile = new PatientProfile();
        //InitializeButtons();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PatientInfoChanged()
    {
        OnChangeID();
        OnChangeAge();
        OnChangeStrokeDate();
        OnChangeAffectedSide();
        OnChangeHandedness();
        OnChangeCategory();
        if(AppSettings.DebugMode) Debug.Log("PatientInfoChanged Called");
    }

    public void InitializeButtons()
    {
        InitializeIDField();
        InitializeAgeField();
        InitializeCategoryDropdown();
        InitializeStrokeDateSelector();
        InitializeAffectedSideDropdown();
        InitializeHandednessDropdown();

        PatientInfoChanged(); // Calls to match displayed and background info, and check Info Coherence

        if (AppSettings.DebugMode) Debug.Log("Editing Buttons initialized");
    }

    public void DownloadOverseerProfile()
    {
        this.editedProfile.CopyFrom(Overseer.Instance.getProfile());
    }

    public void UploadNewProfile()
    {
        editedProfile = new PatientProfile(ID, age, category, timeOfStroke, affectedSide, handedness);
        UpdateOverseerProfile();
    }

    private void UpdateOverseerProfile()
    {
        Overseer.Instance.SetPatientProfile(editedProfile);
        if (AppSettings.DebugMode) Debug.Log("Overseer Patient Profile updated");
        if (AppSettings.DebugMode) Overseer.Instance.getProfile().PrintInfo();
    }

#endregion

    #region Update Profile Helpers
    // helper functions, to modify to act on possible issues
    public void OnChangeID()
    {
        ID = IDField.text;
    }

    public void OnChangeAge()
    {
        //age = int.Parse(ageField.text.Replace("\u200B", ""));
        // prevent issues from initialized input fields
        if (ageField.text != "")
        {
            int.TryParse(ageField.text.Replace("\u200B", ""), out age);
        }

        /*
        Debug.Log(ageField.text == "");
        int parsedAge;
        string cleanedInput = ageField.text.Replace("\u200B", "").Trim();

        if (int.TryParse(cleanedInput, out parsedAge))
        {
            age = parsedAge;
        }
        else
        {
            Debug.LogWarning("Invalid age input: " + ageField.text);
        }
        */
    }

    public void OnChangeCategory()
    {
        ChangeCategory();

        UpdateCategoryDependentVariables();
        UpdateValidityStatus();
    }

    private void ChangeCategory()
    {
        // have to use the indexing of the dropdown
        int chosenEntryIndex = categoryDropdown.value;

        // recover the enum-type value using the index
        category = (PatientProfile.StudyCategory)Enum.Parse(typeof(PatientProfile.StudyCategory), categoryDropdown.options[chosenEntryIndex].text);
    }



    public void OnChangeStrokeDate()
    {
        if (AppSettings.DebugMode) Debug.Log("Changing date of Stroke:");
        isStrokeDateFormatValid = ChangeTimeOfStroke();
        UpdateValidityStatus();
    }

    private bool ChangeTimeOfStroke()
    {
        int chosenDayIndex = timeOfStrokeChoice.DayChoice.value;
        int chosenMonthIndex = timeOfStrokeChoice.MonthChoice.value;
        int chosenYearIndex = timeOfStrokeChoice.YearChoice.value;

        string chosenDayText = timeOfStrokeChoice.DayChoice.options[chosenDayIndex].text;
        string chosenMonthText = timeOfStrokeChoice.MonthChoice.options[chosenMonthIndex].text;
        string chosenYearText = timeOfStrokeChoice.YearChoice.options[chosenYearIndex].text;

        /*
        int chosenDayInt = int.Parse(chosenDayText);
        int chosenMonthInt = int.Parse(chosenMonthText);
        int chosenYearInt = int.Parse(chosenYearText);
        */
        //string chosenDate = timeOfStrokeChoice.DayChoice.options[chosenDayIndex].text + timeOfStrokeChoice.MonthChoice.options[chosenMonthIndex].text + timeOfStrokeChoice.YearChoice.options[chosenYearIndex].text;

        string formatedDate = $"{chosenDayText}-{chosenMonthText}-{chosenYearText}";

        DateTime chosenDate;
        if (DateTime.TryParse(formatedDate, out chosenDate))
        {
            timeOfStroke = chosenDate;
            Debug.Log("DateofStroke changed successfully to " + timeOfStroke.ToString("dd MM, yyyy"));
            return true;
        }
        else
        {
            Debug.Log($"Unvalid date entered: \"{formatedDate}\"");
            
            errorWindow.text = $"Unvalid date entered: \"{formatedDate}\"";

            return false;
        }
        /*
        Debug.Log($"proposed values :{chosenDayText},{chosenMonthText},{chosenYearText}.");
        timeOfStroke = new DateTime(chosenYearInt, chosenMonthInt, chosenDayInt);
        Debug.Log("Date of Stroke has been updated to " + timeOfStroke.ToString("dd MM, yyyy") + ": " + timeOfStroke.ToString("MMMM dd yyyy"));
        */
    }

    public void OnChangeAffectedSide()
    {
        if (AppSettings.DebugMode) Debug.Log("Changing Affected Side.");
        ChangeAffectedSide();
        UpdateValidityStatus();
    }

    private void ChangeAffectedSide()
    {
        // have to use the indexing of the dropdown
        int chosenEntryIndex = affectedSideDropdown.value;

        // recover the enum-type value using the index
        affectedSide = (PatientProfile.Side)Enum.Parse(typeof(PatientProfile.Side), affectedSideDropdown.options[chosenEntryIndex].text);
        if (AppSettings.DebugMode) Debug.Log("Changed Affected Side to " + affectedSide + ".");
    }

    public void OnChangeHandedness()
    {
        if (AppSettings.DebugMode) Debug.Log("Changing Handedness.");
        ChangeHandedness();
        UpdateValidityStatus();
    }

    private void ChangeHandedness()
    {
        // have to use the indexing of the dropdown
        int chosenEntryIndex = handednessChoice.value;

        // recover the enum-type value using the index
        handedness = (PatientProfile.Side)Enum.Parse(typeof(PatientProfile.Side), handednessChoice.options[chosenEntryIndex].text);
        if (AppSettings.DebugMode) Debug.Log("Changed Handedness to " + handedness + ".");
    }

    private void UpdateCategoryDependentVariables()
    {
        if (AppSettings.DebugMode) Debug.Log("Changing the values of variables that depend on patient's category.");

        if (category == PatientProfile.StudyCategory.Control)
        {
            // Deactivate the strokDateSelector UI component
            
            foreach (var selector in timeOfStrokeChoice.AllElements)
            {
                selector.interactable = false;
            }
            
                // Deactivates the Button object containing the selector for UI clarity
            timeOfStrokeEditor.interactable = false;

            // Deactivate the AffectedSideSelector UI component
            affectedSideDropdown.interactable = false;
                // Deactivates the Button object containing the selector for UI clarity
            affectedSideEditor.interactable = false;

            // Set strokeDate to null or equivalent
            timeOfStroke = DateTime.MinValue;

            // Set affectedSide to null or equivalent
            affectedSide = PatientProfile.Side.None;
        }
        else if (category == PatientProfile.StudyCategory.Stroke)
        {
            // Reactivate the strokDateSelector UI component
            foreach (var selector in timeOfStrokeChoice.AllElements)
            {
                selector.interactable = true;
            }
                // Reactivates the Button object containing the selector for UI clarity
            timeOfStrokeEditor.interactable = true;

            // Reactivate the AffectedSideSelector UI component
            affectedSideDropdown.interactable = true;
                // Reactivates the Button object containing the selector for UI clarity
            affectedSideEditor.interactable = true;

            // Set strokeDate to last selected value
            OnChangeStrokeDate();

            // Set affectedSide to last selected value
            OnChangeAffectedSide();
        }
    }

#endregion

#region Information Coherence Check

    private void UpdateValidityStatus()
    {
        if (AppSettings.DebugMode) Debug.Log("Checking Validity and coherence of all selected information");
        UpdateCategoryLinkedValidity();

        // Checks that no information entered will occur an error
        bool canProceedToEdit = isStrokeDateValid && isCategoryValid;
        confirmChangesButton.interactable = canProceedToEdit;

        if (canProceedToEdit)
        {
            if(AppSettings.DebugMode) Debug.Log("All informations are acceptable.");
            errorWindow.text = "";
        }
        else
        {
            if (AppSettings.DebugMode)
            {
                string logWarning = "Cannot proceed:";
                if (!isStrokeDateValid)
                    logWarning += "\n      -Stroke date is invalid.";
                if (!isCategoryValid)
                    logWarning += "\n      -Category choice is invalid.";
                if (!isAffectedSideValid)
                    logWarning += "\n      -Affected Side choice is invalid.";
                Debug.LogWarning(logWarning);
            }
        }
    }

    private bool UpdateCategoryLinkedValidity()
    {
        if (AppSettings.DebugMode) Debug.Log("Updating Category-linked validities");

        // Check that patient info is coherent with patient category
        bool strokeDateisNull = timeOfStroke == DateTime.MinValue;
        bool affectedSideisNull = affectedSide == PatientProfile.Side.None;

        UpdateStrokeDateValidity();
        UpdateAffectedSideValidity();

        switch (category)
        {
            case PatientProfile.StudyCategory.Control:
                isCategoryValid = strokeDateisNull && affectedSideisNull;

                if (isCategoryValid)
                {
                    Debug.Log("✔ Control category: stroke date and affected side are correctly null.");
                }
                else
                {
                    string logWarning = "❌ Error in Category Coherence:";
                    if (!strokeDateisNull)
                    {
                        logWarning += $"\n     -Stroke Date should be null, current selection: {timeOfStroke:dd MM, yyyy}.";
                    }
                    if (!affectedSideisNull)
                    {
                        logWarning += $"\n     -Affected Side should be null, current selection: {affectedSide}.";
                    }
                    Debug.LogWarning(logWarning);
                }
                break;

            case PatientProfile.StudyCategory.Stroke:
                isCategoryValid = !strokeDateisNull && !affectedSideisNull;
                if (isCategoryValid)
                {
                    Debug.Log("✔ Control category: stroke date and affected side are not null.");
                }
                else
                {
                    string logWarning = "❌ Error in Category Coherence:";
                    if (strokeDateisNull)
                    {
                        logWarning += "\n      -Stroke date for stroke patient is null.";
                    }
                    if (affectedSideisNull)
                    {
                        logWarning += "\n      -Affected side for stroke patient is null.";
                    }
                    Debug.LogWarning(logWarning);
                }
                break;
            default:
                Debug.LogWarning("❗ Unexpected value during category check: Category is neither Stroke nor Control");
                break;
        }

        return isCategoryValid;
    }

    private void UpdateStrokeDateValidity()
    {
        if (AppSettings.DebugMode) Debug.Log("Updating Stroke Date Validity.");

        bool strokeDateIsNull = timeOfStroke == DateTime.MinValue;
        switch (category)
        {
            case PatientProfile.StudyCategory.Control:
                if (strokeDateIsNull)
                    isStrokeDateValid = true;
                else
                    isStrokeDateValid = false;
                break;
            case PatientProfile.StudyCategory.Stroke:
                if (strokeDateIsNull)
                {
                    isStrokeDateValid = false;
                    errorWindow.text = "Please enter the date of the stroke";
                }
                else if (!isStrokeDateFormatValid)
                    // Errors in format are handled in the ChangeTimeOfStroke() Method during the tryParse(formatedDate)
                    isStrokeDateValid = ChangeTimeOfStroke();
                else
                    isStrokeDateValid = true;
                break;
        }
    }

    private void UpdateAffectedSideValidity()
    {
        if (AppSettings.DebugMode) Debug.Log("Updating Affected Side Validity");

        bool affectedSideIsNull = affectedSide == PatientProfile.Side.None;
        switch (category)
        {
            case PatientProfile.StudyCategory.Control:
                if (affectedSideIsNull)
                    isAffectedSideValid = true;
                else
                    isAffectedSideValid = false;
                break;
            case PatientProfile.StudyCategory.Stroke:
                if (affectedSideIsNull)
                {
                    isAffectedSideValid = false;
                    errorWindow.text = "Please chose an affected side";
                }  
                else
                    isAffectedSideValid = true;
                break;
        }
        if (AppSettings.DebugMode) Debug.Log("Affected side validity is " + isAffectedSideValid);
    }

    #endregion

    #region Initialization Helpers

    private void InitializeIDField()
    {
        IDField.text = editedProfile.ID.ToString();
    }

    private void InitializeAgeField()
    {
        ageField.text = editedProfile.Age.ToString();
    }

    private void InitializeCategoryDropdown()
    {
        string[] arrayOfChoices = Enum.GetNames(typeof(PatientProfile.StudyCategory));
        List<string> listOfChoices = new List<string>(arrayOfChoices);

        categoryDropdown.options.Clear();
        categoryDropdown.AddOptions(listOfChoices);
    }

    private void InitializeAffectedSideDropdown()
    {
        string[] arrayOfChoices = Enum.GetNames(typeof(PatientProfile.Side));
        List<string> listOfChoices = new List<string>(arrayOfChoices);

        affectedSideDropdown.options.Clear();
        affectedSideDropdown.AddOptions(listOfChoices);
    }

    private void InitializeHandednessDropdown()
    {
        string[] arrayOfChoices = Enum.GetNames(typeof(PatientProfile.Side));
        List<string> listOfChoices = new List<string>(arrayOfChoices);

        handednessChoice.options.Clear();
        handednessChoice.AddOptions(listOfChoices);
    }

    public void InitializeStrokeDateSelector()
    {
        foreach (var dropdownElement in timeOfStrokeChoice.AllElements)
        {
            dropdownElement.options.Clear();
        }

        List<string> days = new List<string>();

        for (int day = 1; day <= 31; day++)
        {
            //timeOfStrokeChoice.DayChoice.options.Add(new TMP_Dropdown.OptionData(day.ToString()));
            days.Add(day.ToString());
        }
        timeOfStrokeChoice.DayChoice.AddOptions(days);

        List<string> months = new List<string>();
        for (int month = 1; month <= 12; month++)
        {
            //timeOfStrokeChoice.MonthChoice.options.Add(new TMP_Dropdown.OptionData(month.ToString()));
            months.Add(month.ToString());
        }
        timeOfStrokeChoice.MonthChoice.AddOptions(months);

        List<string> years = new List<string>();
        for (int year = 2025; year >= 1900; year--)
        {
            //timeOfStrokeChoice.YearChoice.options.Add(new TMP_Dropdown.OptionData(year.ToString()));
            years.Add(year.ToString());
        }
        timeOfStrokeChoice.YearChoice.AddOptions(years);

    }

    #endregion

}
