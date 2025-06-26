using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonListButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text myText;
    [SerializeField]
    private ButtonListControl buttonControl;

    private string myTextString;

    public void SetText(string textString)
    {
        myTextString = textString;
        myText.text = textString;
    }

    public void OnClick()
    {
        buttonControl.ButtonClicked(myTextString);
    }
}
