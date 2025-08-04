using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class InteractableOrNot : MonoBehaviour
{
    public Selectable selectable;

    void OnValidate()
    {
        selectable = GetComponent<Selectable>();
    }
    public void SetNotInteractable()
    {
        selectable.interactable = false;
    }

    public void SetYesInteractable()
    {
        selectable.interactable = true;
    }

    public void SetInteractable(bool isInteractable)
    {
        selectable.interactable = isInteractable;
    }

    public void ToggleInteractable()
    {
        selectable.interactable = !selectable.interactable;
    }


}
