using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable
{
    // Lo que pasa en la escena al abrirla (hojas que giran, luz de afuera, pantalla final)
    [SerializeField] private UnityEvent onOpened = new UnityEvent();

    public UnityEvent OnOpened => onOpened;

    public void Interact()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasKey)
        {
            Debug.Log("Necesitas la llave.");
            return;
        }

        Debug.Log("Puerta abierta. ¡Escape completado!");
        onOpened.Invoke();
    }
}
