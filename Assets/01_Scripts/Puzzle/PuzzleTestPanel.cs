using UnityEngine;

public class PuzzleTestPanel : MonoBehaviour
{
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private ChestController chest;
    [SerializeField] private DoorController door;

    private void Start()
    {
        if (door == null)
        {
            door = FindObjectOfType<DoorController>();
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(16f, 16f, 280f, 180f), GUI.skin.box);
        GUILayout.Label("Prueba del puzzle");

        GUI.enabled = puzzleManager != null;
        if (GUILayout.Button("Validar codigo correcto (1234)"))
        {
            puzzleManager.ValidateCode("1234");
        }

        if (GUILayout.Button("Validar codigo incorrecto (0000)"))
        {
            puzzleManager.ValidateCode("0000");
        }

        GUI.enabled = chest != null;
        if (GUILayout.Button("Interactuar con cofre"))
        {
            chest.Interact();
        }

        GUI.enabled = door != null;
        if (GUILayout.Button("Interactuar con puerta"))
        {
            door.Interact();
        }

        GUI.enabled = true;
        GUILayout.EndArea();
    }
}
