using UnityEngine;

// Candado de combinación del cofre. Con E se abre un panel para escribir el código, que se
// comprueba con PuzzleManager.ValidateCode: si es correcto, el cofre se desbloquea y el candado cae.
public class CodeLock : MonoBehaviour, IInteractable
{
    public static bool IsOpen { get; private set; }

    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private int codeLength = 4;
    [SerializeField] private Transform lockVisual;
    // Movimiento, mirada e interacción del jugador: se apagan mientras se escribe el código
    [SerializeField] private Behaviour[] disableWhileOpen = new Behaviour[0];

    private bool open;
    private bool solved;
    private string input = "";
    private string feedback = "";
    private float errorTime = -10f;

    private float dropTimer = -1f;
    private Vector3 dropStart;
    private Quaternion dropRotation;
    private float dropHeight;

    private int stylesHeight = -1;
    private GUIStyle titleStyle, textStyle, digitStyle, feedbackStyle, keyStyle, actionStyle, closeStyle;

    public void Interact()
    {
        if (!solved && !open)
            SetOpen(true);
    }

    private void SetOpen(bool value)
    {
        open = value;
        IsOpen = value;
        input = "";
        feedback = "";

        foreach (Behaviour behaviour in disableWhileOpen)
        {
            if (behaviour != null)
                behaviour.enabled = !value;
        }

        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = value;
    }

    private void OnDestroy()
    {
        if (open)
            IsOpen = false;
    }

    private void Update()
    {
        if (open)
            ReadKeyboard();

        if (dropTimer >= 0f)
            AnimateDrop();
    }

    private void ReadKeyboard()
    {
        for (int digit = 0; digit <= 9; digit++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + digit)) || Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad0 + digit)))
                AddDigit(digit);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
            RemoveDigit();
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Submit();
        if (Input.GetKeyDown(KeyCode.Escape))
            SetOpen(false);
    }

    private void AddDigit(int digit)
    {
        if (input.Length < codeLength)
            input += digit;
        feedback = "";
    }

    private void RemoveDigit()
    {
        if (input.Length > 0)
            input = input.Substring(0, input.Length - 1);
    }

    private void Submit()
    {
        if (input.Length < codeLength)
        {
            feedback = "Faltan dígitos.";
            return;
        }

        if (puzzleManager != null && puzzleManager.ValidateCode(input))
        {
            solved = true;
            SetOpen(false);
            Collider lockCollider = GetComponent<Collider>();
            if (lockCollider != null)
                lockCollider.enabled = false;
            StartDrop();
        }
        else
        {
            errorTime = Time.time;
            feedback = "Código incorrecto.";
            input = "";
        }
    }

    // El candado abierto se suelta y cae al piso
    private void StartDrop()
    {
        if (lockVisual == null)
            return;

        dropStart = lockVisual.position;
        dropRotation = lockVisual.rotation;
        dropHeight = Mathf.Max(0f, dropStart.y - 0.04f);
        dropTimer = 0f;
    }

    private void AnimateDrop()
    {
        dropTimer += Time.deltaTime;
        float t = Mathf.Clamp01(dropTimer / 0.45f);
        lockVisual.position = dropStart + transform.forward * 0.12f * t + Vector3.down * dropHeight * t * t;
        lockVisual.rotation = dropRotation * Quaternion.Euler(80f * t, 0f, 0f);

        if (t >= 1f)
            dropTimer = -1f;
    }

    private void BuildStyles()
    {
        if (stylesHeight == Screen.height)
            return;

        stylesHeight = Screen.height;
        titleStyle = HudStyle.Text(0.034f, HudStyle.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
        textStyle = HudStyle.Text(0.022f, HudStyle.Paper, TextAnchor.MiddleCenter);
        digitStyle = HudStyle.Text(0.06f, HudStyle.Paper, TextAnchor.MiddleCenter, FontStyle.Bold);
        feedbackStyle = HudStyle.Text(0.024f, HudStyle.Wrong, TextAnchor.MiddleCenter, FontStyle.Bold);
        keyStyle = HudStyle.Button(0.036f, new Color(0.2f, 0.15f, 0.11f), HudStyle.Paper);
        actionStyle = HudStyle.Button(0.024f, new Color(0.45f, 0.33f, 0.16f), Color.white);
        closeStyle = HudStyle.Button(0.02f, new Color(0.16f, 0.12f, 0.1f), HudStyle.Paper);
    }

    private void OnGUI()
    {
        if (!open)
            return;

        GUI.depth = -10; // por encima del HUD
        BuildStyles();

        float screenW = Screen.width;
        float screenH = Screen.height;
        HudStyle.Box(new Rect(0f, 0f, screenW, screenH), HudStyle.Shade);

        float w = screenH * 0.5f;
        float h = screenH * 0.78f;
        var panel = new Rect((screenW - w) / 2f, (screenH - h) / 2f, w, h);
        HudStyle.Frame(panel, HudStyle.Panel, HudStyle.Gold, Mathf.Max(2f, screenH / 360f));

        float y = panel.y + h * 0.04f;
        GUI.Label(new Rect(panel.x, y, w, h * 0.07f), "CANDADO DEL COFRE", titleStyle);
        y += h * 0.07f;
        GUI.Label(new Rect(panel.x, y, w, h * 0.05f), "Escribe los " + codeLength + " dígitos de la combinación", textStyle);
        y += h * 0.08f;

        // Ruedas con los dígitos escritos (tiemblan en rojo si el código es incorrecto)
        bool wrong = Time.time - errorTime < 0.6f;
        float shake = wrong ? Mathf.Sin(Time.time * 60f) * w * 0.015f : 0f;
        float slot = w * 0.16f;
        float gap = w * 0.04f;
        float slotsX = panel.x + (w - codeLength * slot - (codeLength - 1) * gap) / 2f + shake;
        for (int i = 0; i < codeLength; i++)
        {
            var slotRect = new Rect(slotsX + i * (slot + gap), y, slot, slot * 1.25f);
            HudStyle.Frame(slotRect, new Color(0.13f, 0.1f, 0.08f), wrong ? HudStyle.Wrong : new Color(0.55f, 0.42f, 0.22f), 2f);
            string shown = i < input.Length ? input[i].ToString() : "·";
            GUI.Label(slotRect, shown, digitStyle);
        }
        y += slot * 1.25f + h * 0.02f;

        GUI.Label(new Rect(panel.x, y, w, h * 0.05f), feedback, feedbackStyle);
        y += h * 0.06f;

        // Teclado numérico
        float key = w * 0.22f;
        float keyH = h * 0.085f;
        float keyGap = w * 0.03f;
        float keysX = panel.x + (w - 3f * key - 2f * keyGap) / 2f;
        string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "Borrar", "0", "Probar" };
        for (int i = 0; i < labels.Length; i++)
        {
            var keyRect = new Rect(keysX + (i % 3) * (key + keyGap), y + (i / 3) * (keyH + keyGap * 0.6f), key, keyH);
            bool isAction = i == 9 || i == 11;
            if (GUI.Button(keyRect, labels[i], isAction ? actionStyle : keyStyle))
            {
                if (i == 9) RemoveDigit();
                else if (i == 11) Submit();
                else AddDigit(int.Parse(labels[i]));
            }
        }
        y += 4f * (keyH + keyGap * 0.6f) + h * 0.01f;

        if (GUI.Button(new Rect(panel.x + w * 0.3f, y, w * 0.4f, h * 0.055f), "Cerrar (Esc)", closeStyle))
            SetOpen(false);
    }
}
