using UnityEngine;

// La llave sale flotando del cofre, gira y desaparece: el jugador ya la tiene (GameManager.HasKey).
public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float delay = 0.9f;
    [SerializeField] private float riseHeight = 0.7f;
    [SerializeField] private float duration = 1.8f;

    private float timer = -1f;
    private Vector3 startPosition;
    private Vector3 startScale;

    public void Collect()
    {
        if (timer >= 0f)
            return;

        timer = 0f;
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    private void Update()
    {
        if (timer < 0f)
            return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01((timer - delay) / duration);
        if (t <= 0f)
            return;

        transform.position = startPosition + Vector3.up * riseHeight * Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t * 1.5f));
        transform.Rotate(0f, 220f * Time.deltaTime, 0f, Space.World);

        if (t > 0.7f)
            transform.localScale = startScale * Mathf.InverseLerp(1f, 0.7f, t);

        if (t >= 1f)
            gameObject.SetActive(false);
    }
}
