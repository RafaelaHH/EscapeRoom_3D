using UnityEngine;

// WIP: temporizador del puzzle, pendiente de definir con el equipo.
public class PuzzleTimer : MonoBehaviour
{
    [SerializeField] private float timeLimit = 60f;

    private float elapsed;

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= timeLimit)
        {
            Debug.Log("Tiempo agotado.");
            enabled = false;
        }
    }
}
