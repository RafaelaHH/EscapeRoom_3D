using UnityEngine;

// Objeto del cuarto que esconde parte del código del candado. Al examinarlo (E) muestra su pista.
public class ClueObject : MonoBehaviour, IInteractable
{
    [SerializeField, TextArea(2, 5)] private string clue;

    public void Interact()
    {
        // GameHUD muestra en pantalla los mensajes de la Console
        Debug.Log(clue);
    }
}
