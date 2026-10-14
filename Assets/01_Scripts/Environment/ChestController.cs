using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    [SerializeField] private bool unlocked;

    public void Unlock()
    {
        unlocked = true;
        Debug.Log("Cofre desbloqueado.");
    }

    public void Interact()
    {
        Debug.Log(unlocked ? "Cofre abierto." : "El cofre está cerrado.");
    }
}
