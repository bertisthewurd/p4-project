using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string promptMessage;

    public void BaseInteract()
    {
        if (Time.timeScale == 0) return;
        Interact();
    }

    protected virtual void Interact()
    {

    }
   
}
