using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool HasKey { get; private set; }
    public int InitializeCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Reinicia el estado de la partida al cargar la escena
        GameManager.Instance.Initialize();
        Debug.Log("GameManager listo.");
    }

    public void Initialize()
    {
        InitializeCount++;
        HasKey = false;
        Debug.Log("GameManager.Initialize() ejecutado. Veces: " + InitializeCount);

        if (InitializeCount > 1)
        {
            Debug.LogWarning("Initialize() se ejecutó más de una vez: el estado del juego se reinició duplicado.");
        }
    }

    public void ObtainKey()
    {
        HasKey = true;
        Debug.Log("Llave obtenida.");
    }
}
