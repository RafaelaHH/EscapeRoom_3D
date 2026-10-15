using UnityEngine;

// Gira una o varias bisagras cuando se llama a Open(): la tapa del cofre o las dos hojas de la puerta.
public class HingeAnimator : MonoBehaviour
{
    [System.Serializable]
    public class Hinge
    {
        public Transform pivot;
        public Vector3 openAngles; // giro, en grados, desde la posición cerrada
    }

    [SerializeField] private Hinge[] hinges = new Hinge[0];
    [SerializeField] private float duration = 1.4f;
    [SerializeField] private float delay = 0f;

    private Quaternion[] closedRotations;
    private float timer = -1f;

    private void Awake()
    {
        closedRotations = new Quaternion[hinges.Length];
        for (int i = 0; i < hinges.Length; i++)
            closedRotations[i] = hinges[i].pivot.localRotation;
    }

    public void Open()
    {
        if (timer < 0f)
            timer = 0f;
    }

    private void Update()
    {
        if (timer < 0f || timer > delay + duration)
            return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01((timer - delay) / duration);
        float eased = 1f - Mathf.Pow(1f - t, 3f); // arranca rápido y frena al final

        for (int i = 0; i < hinges.Length; i++)
            hinges[i].pivot.localRotation = closedRotations[i] * Quaternion.Euler(hinges[i].openAngles * eased);
    }
}
