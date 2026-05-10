using UnityEngine;

/// <summary>
/// Смеситель цветов в 2D-сцене.
///
/// Читает значения двух slot-objectId из WorldState, вычисляет смесь и:
/// 1. Применяет результат к centralSpriteRenderer (визуальный квадрат в 2D).
/// 2. Записывает результат в WorldState под outputObjectId.
///
/// Пересчитывает каждый Update — это гарантирует мгновенную реакцию на изменения слотов.
/// </summary>
public class ColorMixer : MonoBehaviour
{
    [Header("Source Slots")]
    [SerializeField] private string slotAObjectId;
    [SerializeField] private string slotBObjectId;
    [SerializeField] private string slotPropertyType = "Active";
    [SerializeField] private string defaultIfEmpty = "empty";

    [Header("Output")]
    [SerializeField] private string outputObjectId;
    [SerializeField] private string outputPropertyType = "Color";

    [Header("Visual")]
    [SerializeField] private SpriteRenderer centralSpriteRenderer;

    [Header("Color Palette")]
    [SerializeField] private Color colorRed = new Color(0.9f, 0.15f, 0.15f);
    [SerializeField] private Color colorBlue = new Color(0.15f, 0.3f, 0.9f);
    [SerializeField] private Color colorYellow = new Color(1f, 0.9f, 0.1f);
    [SerializeField] private Color colorPurple = new Color(0.6f, 0.2f, 0.8f);
    [SerializeField] private Color colorOrange = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private Color colorGreen = new Color(0.2f, 0.75f, 0.25f);
    [SerializeField] private Color colorWhite = Color.white;

    private string lastMixed = "";

    private void Update()
    {
        if (GameManager.Instance == null) return;

        var ws = GameManager.Instance.World;
        string a = ws.GetString(slotAObjectId, slotPropertyType, defaultIfEmpty);
        string b = ws.GetString(slotBObjectId, slotPropertyType, defaultIfEmpty);

        string mixed = MixColors(a, b);

        // Применяем только если значение изменилось
        if (mixed == lastMixed) return;
        lastMixed = mixed;

        if (centralSpriteRenderer != null)
            centralSpriteRenderer.color = StringToColor(mixed);

        if (!string.IsNullOrEmpty(outputObjectId))
            ws.SetString(outputObjectId, outputPropertyType, mixed);
    }

    public static string MixColors(string a, string b)
    {
        a = Normalize(a);
        b = Normalize(b);

        if (a == "empty" || b == "empty") return "white";
        if (a == b) return a;

        string first = a.CompareTo(b) < 0 ? a : b;
        string second = a.CompareTo(b) < 0 ? b : a;

        if (first == "blue" && second == "red") return "purple";
        if (first == "blue" && second == "yellow") return "green";
        if (first == "red" && second == "yellow") return "orange";

        return "white";
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "empty";
        return s.Trim().ToLowerInvariant();
    }

    public Color StringToColor(string colorName)
    {
        switch (Normalize(colorName))
        {
            case "red": return colorRed;
            case "blue": return colorBlue;
            case "yellow": return colorYellow;
            case "purple": return colorPurple;
            case "orange": return colorOrange;
            case "green": return colorGreen;
            case "white":
            default: return colorWhite;
        }
    }
}
