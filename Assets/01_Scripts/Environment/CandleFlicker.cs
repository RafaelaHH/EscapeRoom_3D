using UnityEngine;

// Hace titilar la luz de una vela o lámpara de aceite.
[RequireComponent(typeof(Light))]
public class CandleFlicker : MonoBehaviour
{
    [SerializeField] private float amount = 0.2f;
    [SerializeField] private float speed = 3f;

    private Light candleLight;
    private float baseIntensity;
    private float seed;

    private void Awake()
    {
        candleLight = GetComponent<Light>();
        baseIntensity = candleLight.intensity;
        seed = Random.value * 100f;
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(seed, Time.time * speed);
        candleLight.intensity = baseIntensity * (1f - amount + 2f * amount * noise);
    }
}
