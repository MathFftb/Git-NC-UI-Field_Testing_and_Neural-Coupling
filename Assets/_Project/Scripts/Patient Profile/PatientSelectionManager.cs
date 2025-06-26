using UnityEngine;
using UnityEngine.SceneManagement;

public class PatientSelectionManager : MonoBehaviour
{

    [SerializeField] private EditButton editButton;

    private ButtonListControl patientList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ConfirmButtonClicked()
    {
        SceneManager.LoadScene(2);
    }

}
