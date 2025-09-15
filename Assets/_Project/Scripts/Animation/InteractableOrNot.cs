using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attaching this script to a gameobject allows you to regulate their interactibility from any script or Button.
/// </summary>
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
