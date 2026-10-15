using System.Collections.Generic;
using UnityEngine;

// Colores y estilos compartidos por el HUD y el candado (los dos se dibujan con OnGUI).
public static class HudStyle
{
    public static readonly Color Gold = new Color(0.96f, 0.8f, 0.47f);
    public static readonly Color Paper = new Color(0.94f, 0.9f, 0.82f);
    public static readonly Color Panel = new Color(0.07f, 0.05f, 0.04f, 0.9f);
    public static readonly Color Shade = new Color(0f, 0f, 0f, 0.55f);
    public static readonly Color Wrong = new Color(0.92f, 0.32f, 0.26f);

    private static readonly Dictionary<Color, Texture2D> textures = new Dictionary<Color, Texture2D>();

    // Textura de un solo color para pintar fondos
    public static Texture2D Solid(Color color)
    {
        if (!textures.TryGetValue(color, out Texture2D texture) || texture == null)
        {
            texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            textures[color] = texture;
        }
        return texture;
    }

    public static void Box(Rect rect, Color color)
    {
        GUI.DrawTexture(rect, Solid(color));
    }

    // Recuadro con borde (para paneles)
    public static void Frame(Rect rect, Color fill, Color border, float thickness)
    {
        Box(rect, border);
        Box(new Rect(rect.x + thickness, rect.y + thickness, rect.width - 2f * thickness, rect.height - 2f * thickness), fill);
    }

    // Tamaño de letra proporcional al alto de la pantalla
    public static int FontSize(float fraction)
    {
        return Mathf.Max(10, Mathf.RoundToInt(Screen.height * fraction));
    }

    public static GUIStyle Text(float size, Color color, TextAnchor anchor, FontStyle fontStyle = FontStyle.Normal)
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = FontSize(size),
            alignment = anchor,
            fontStyle = fontStyle,
            wordWrap = true,
            clipping = TextClipping.Overflow
        };
        style.normal.textColor = color;
        return style;
    }

    public static GUIStyle Button(float size, Color background, Color textColor)
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = FontSize(size),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(0, 0, 0, 0)
        };
        style.normal.background = Solid(background);
        style.hover.background = Solid(Color.Lerp(background, Color.white, 0.15f));
        style.active.background = Solid(Color.Lerp(background, Color.black, 0.25f));
        style.normal.textColor = style.hover.textColor = style.active.textColor = textColor;
        return style;
    }

    // Texto con sombra, para que se lea sobre cualquier fondo
    public static void Shadowed(Rect rect, string text, GUIStyle style)
    {
        Color color = style.normal.textColor;
        style.normal.textColor = new Color(0f, 0f, 0f, 0.85f * color.a);
        float offset = Mathf.Max(1f, Screen.height / 540f);
        GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);
        style.normal.textColor = color;
        GUI.Label(rect, text, style);
    }
}
