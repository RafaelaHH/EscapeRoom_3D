using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// HUD del escape room (OnGUI): mira, aviso "[E] ...", mensajes, llave, tiempo y pantalla final.
// Los mensajes salen de Debug.Log: así lo que ya escriben el cofre, la puerta y el puzzle
// en la Console también se ve en pantalla, sin cambiar esos scripts.
public class GameHUD : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 2f; // el mismo valor que PlayerInteraction
    [SerializeField] private string roomTitle = "El Estudio del Coleccionista";
    [SerializeField, TextArea(2, 4)] private string objective =
        "La puerta está cerrada con llave. Busca la nota del coleccionista.";
    [SerializeField] private string controls = "WASD moverse · Ratón o Q/R girar · E interactuar · Shift correr";
    [SerializeField] private Behaviour[] disableOnVictory = new Behaviour[0];
    // Mensajes técnicos que se quedan solo en la Console
    [SerializeField] private string[] hiddenMessages =
        { "Movement system", "Interaction key pressed", "GameManager", "Interactuando con" };

    private struct Message
    {
        public string text;
        public float start;
        public float end;
    }

    private readonly List<Message> messages = new List<Message>();
    private float startTime;
    private float victoryTime = -1f;
    private string prompt;

    private int stylesHeight = -1;
    private GUIStyle titleStyle, objectiveStyle, messageStyle, promptStyle, smallStyle, keyStyle, victoryStyle;

    private void OnEnable()
    {
        Application.logMessageReceived += OnLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= OnLog;
    }

    private void Start()
    {
        startTime = Time.time;
    }

    private void OnLog(string text, string stackTrace, LogType type)
    {
        if (type != LogType.Log || string.IsNullOrEmpty(text))
            return;

        foreach (string hidden in hiddenMessages)
        {
            if (text.StartsWith(hidden))
                return;
        }

        float now = Time.time;
        messages.Add(new Message { text = text, start = now, end = now + 2.5f + text.Length * 0.05f });
        if (messages.Count > 3)
            messages.RemoveAt(0);
    }

    // La llama la puerta al abrirse (evento OnOpened de DoorController)
    public void ShowVictory()
    {
        if (victoryTime >= 0f)
            return;

        victoryTime = Time.time;
        foreach (Behaviour behaviour in disableOnVictory)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        prompt = null;
        messages.RemoveAll(m => Time.time > m.end);

        if (victoryTime >= 0f)
        {
            if (Time.time - victoryTime > 3f && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (player != null && !CodeLock.IsOpen)
            prompt = FindPrompt();
    }

    // Mismo criterio que PlayerInteraction: el objeto interactuable más cercano
    private string FindPrompt()
    {
        Collider closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in Physics.OverlapSphere(player.position, interactionDistance))
        {
            if (hit.GetComponent<IInteractable>() == null)
                continue;

            float distance = Vector3.Distance(player.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closest = hit;
                closestDistance = distance;
            }
        }

        if (closest == null)
            return null;

        InteractionPrompt label = closest.GetComponent<InteractionPrompt>();
        return label != null ? label.Text : "Interactuar";
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    private void BuildStyles()
    {
        if (stylesHeight == Screen.height)
            return;

        stylesHeight = Screen.height;
        titleStyle = HudStyle.Text(0.06f, HudStyle.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
        objectiveStyle = HudStyle.Text(0.026f, HudStyle.Paper, TextAnchor.MiddleCenter);
        messageStyle = HudStyle.Text(0.025f, HudStyle.Paper, TextAnchor.MiddleCenter);
        promptStyle = HudStyle.Text(0.026f, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        smallStyle = HudStyle.Text(0.018f, new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleLeft);
        keyStyle = HudStyle.Text(0.022f, HudStyle.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
        victoryStyle = HudStyle.Text(0.1f, HudStyle.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
    }

    private void OnGUI()
    {
        BuildStyles();
        float w = Screen.width;
        float h = Screen.height;
        float now = Time.time;

        if (victoryTime >= 0f)
        {
            DrawVictory(w, h, now);
            return;
        }

        // Mira en el centro
        if (!CodeLock.IsOpen)
        {
            float dot = Mathf.Max(4f, h * 0.006f);
            HudStyle.Box(new Rect((w - dot) / 2f, (h - dot) / 2f, dot, dot), new Color(1f, 1f, 1f, 0.7f));
        }

        // Título del cuarto al empezar
        float intro = now - startTime;
        if (intro < 9f)
        {
            float alpha = Mathf.Clamp01(intro / 1.2f) * Mathf.Clamp01((9f - intro) / 1.5f);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            HudStyle.Box(new Rect(0f, h * 0.12f, w, h * 0.2f), new Color(0f, 0f, 0f, 0.45f));
            HudStyle.Shadowed(new Rect(0f, h * 0.13f, w, h * 0.1f), roomTitle, titleStyle);
            HudStyle.Shadowed(new Rect(w * 0.15f, h * 0.23f, w * 0.7f, h * 0.07f), objective, objectiveStyle);
            GUI.color = Color.white;
        }

        // Tiempo y controles
        HudStyle.Shadowed(new Rect(h * 0.03f, h * 0.02f, w * 0.4f, h * 0.04f), "Tiempo  " + FormatTime(now - startTime), smallStyle);
        HudStyle.Shadowed(new Rect(h * 0.03f, h * 0.94f, w * 0.9f, h * 0.04f), controls, smallStyle);

        // Llave en el inventario
        if (GameManager.Instance != null && GameManager.Instance.HasKey)
        {
            var keyRect = new Rect(w - h * 0.33f, h * 0.025f, h * 0.3f, h * 0.06f);
            HudStyle.Frame(keyRect, HudStyle.Panel, HudStyle.Gold, 2f);
            GUI.Label(keyRect, "LLAVE DE LA SALIDA", keyStyle);
        }

        // Aviso del objeto que se puede usar
        if (!string.IsNullOrEmpty(prompt))
        {
            string text = "[E]  " + prompt;
            Vector2 size = promptStyle.CalcSize(new GUIContent(text));
            var promptRect = new Rect((w - size.x) / 2f - h * 0.02f, h * 0.6f, size.x + h * 0.04f, size.y + h * 0.015f);
            HudStyle.Box(promptRect, new Color(0f, 0f, 0f, 0.55f));
            GUI.Label(promptRect, text, promptStyle);
        }

        DrawMessages(w, h, now);
    }

    // Mensajes del juego, el más nuevo abajo
    private void DrawMessages(float w, float h, float now)
    {
        float maxWidth = w * 0.6f;
        float bottom = h * 0.9f;
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            Message message = messages[i];
            float alpha = Mathf.Clamp01((now - message.start) / 0.25f) * Mathf.Clamp01((message.end - now) / 0.6f);
            var content = new GUIContent(message.text);
            float width = Mathf.Min(maxWidth, messageStyle.CalcSize(content).x) + h * 0.05f;
            float height = messageStyle.CalcHeight(content, width - h * 0.05f) + h * 0.02f;
            var rect = new Rect((w - width) / 2f, bottom - height, width, height);

            GUI.color = new Color(1f, 1f, 1f, alpha);
            HudStyle.Frame(rect, HudStyle.Panel, new Color(0.55f, 0.42f, 0.22f), 1f);
            GUI.Label(new Rect(rect.x + h * 0.025f, rect.y, rect.width - h * 0.05f, rect.height), message.text, messageStyle);
            GUI.color = Color.white;

            bottom -= height + h * 0.01f;
        }
    }

    private void DrawVictory(float w, float h, float now)
    {
        // Espera a que se vea la puerta abriéndose
        float alpha = Mathf.Clamp01((now - victoryTime - 1.5f) / 1.2f);
        if (alpha <= 0f)
            return;

        GUI.color = new Color(1f, 1f, 1f, alpha);
        HudStyle.Box(new Rect(0f, 0f, w, h), new Color(0f, 0f, 0f, 0.6f));
        HudStyle.Shadowed(new Rect(0f, h * 0.3f, w, h * 0.15f), "¡ESCAPASTE!", victoryStyle);
        HudStyle.Shadowed(new Rect(0f, h * 0.46f, w, h * 0.06f),
            roomTitle + "  ·  Tiempo final " + FormatTime(victoryTime - startTime), objectiveStyle);
        if (now - victoryTime > 3f)
            HudStyle.Shadowed(new Rect(0f, h * 0.56f, w, h * 0.05f), "Pulsa ENTER para jugar otra vez", keyStyle);
        GUI.color = Color.white;
    }
}
