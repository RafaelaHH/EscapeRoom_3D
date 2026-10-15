using UnityEngine;

// Texto que el HUD muestra cuando el jugador está cerca de un objeto: "[E] Examinar el reloj".
public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private string text = "Interactuar";

    public string Text => text;
}
