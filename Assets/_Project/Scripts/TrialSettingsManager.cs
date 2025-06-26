using UnityEngine;
using UnityEngine.SceneManagement;

public class TrialSettingsManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReturnButtonClicked()
    {
        LoadPatientSelectionScene();
    }

    public void ConfirmButtonClicked()
    {
        LoadRunScreen();
    }

    public void LoadPatientSelectionScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadRunScreen()
    {
        SceneManager.LoadScene(3);
    }
}
