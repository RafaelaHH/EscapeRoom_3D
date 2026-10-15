using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasKey)
        {
            Debug.Log("Necesitas la llave.");
            return;
        }

        Debug.Log("Puerta abierta. ¡Escape completado!");
    }
}
