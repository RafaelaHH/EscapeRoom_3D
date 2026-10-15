using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChestController : MonoBehaviour, IInteractable
{
    [SerializeField] private bool unlocked;
    // Lo que pasa en la escena al abrirlo (tapa que se levanta, llave que sale)
    [SerializeField] private UnityEvent onOpened = new UnityEvent();
    private bool opened;

    public UnityEvent OnOpened => onOpened;

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
        onOpened.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ObtainKey();
        }
    }
}
