using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldDoorController : MonoBehaviour
{
    public bool isOpen;

    public void Open()
    {
        isOpen = true;
        Debug.Log("Puerta antigua abierta.");
        Debug.Log("Controlador antiguo actualizado.");
    }
}
