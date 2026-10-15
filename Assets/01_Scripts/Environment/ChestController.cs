using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour, IInteractable
{
    [SerializeField] private bool unlocked;
    private bool opened;

    public void Unlock()
    {
        unlocked = true;
        Debug.Log("Cofre desbloqueado.");
    }

    public void Interact()
    {
        if (!unlocked)
        {
            Debug.Log("El cofre está cerrado.");
            return;
        }

        if (opened)
        {
            Debug.Log("El cofre ya está abierto.");
            return;
        }

        opened = true;
        Debug.Log("Cofre abierto.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ObtainKey();
        }
    }
}
