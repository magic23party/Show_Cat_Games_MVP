using UnityEngine;

/// <summary>
/// Смеситель цветов в 2D-сцене.
/// 
/// Читает значения двух slot-objectId из WorldState (например "mixer_1_a", "mixer_1_b"),
/// вычисляет смесь по таблице, и:
/// 1. Применяет результат к centralSpriteRenderer (визуальный квадрат в 2D).
/// 2. Записывает результат в WorldState под outputObjectId как Color (например "cube_1:Color = purple").
///
/// Команды-цвета имеют commandValue: "red", "blue", "yellow".
/// Если в слоте ничего нет — он всё ещё содержит последнее значение, ИЛИ defaultIfEmpty.
/// </summary>
public class ColorMixer : MonoBehaviour
{
    [Header("Source Slots")]
    [Tooltip("Object ID первого слота (например, 'mixer_1_a').")]
    [SerializeField] private string slotAObjectId;

    [Tooltip("Object ID второго слота (например, 'mixer_1_b').")]
    [SerializeField] private string slotBObjectId;

    [Tooltip("Property type, который слоты пишут в WorldState. Обычно 'Active'.")]
    [SerializeField] private string slotPropertyType = "Active";

    [Tooltip("Если слот не содержит цвета (например, пустой при старте) — что считать. " +
             "Поставь 'empty' (любое слово, не red/blue/yellow) — тогда смесь будет 'white'.")]
    [SerializeField] private string defaultIfEmpty = "empty";

    [Header("Output")]
    [Tooltip("Object ID результата (например, 'cube_1'). HackableColor с этим ID должен висеть на 3D-кубе.")]
    [SerializeField] private string outputObjectId;

    [Tooltip("Property type для результата. Обычно 'Color'.")]
    [SerializeField] private string outputPropertyType = "Color";

    [Header("Visual (центральный квадрат в 2D)")]
    [Tooltip("SpriteRenderer центрального квадрата смесителя. Будет менять color.")]
    [SerializeField] private SpriteRenderer centralSpriteRenderer;

    [Header("Color Palette (можешь подправить)")]
    [SerializeField] private Color colorRed = new Color(0.9f, 0.15f, 0.15f);
    [SerializeField] private Color colorBlue = new Color(0.15f, 0.3f, 0.9f);
    [SerializeField] private Color colorYellow = new Color(1f, 0.9f, 0.1f);
    [SerializeField] private Color colorPurple = new Color(0.6f, 0.2f, 0.8f);
    [SerializeField] private Color colorOrange = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private Color colorGreen = new Color(0.2f, 0.75f, 0.25f);
    [SerializeField] private Color colorWhite = Color.white;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        Recompute();

        // Подписываемся на любые изменения string в WorldState
        GameManager.Instance.World.OnStringChanged += HandleChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.World.OnStringChanged -= HandleChanged;
    }

    private void HandleChanged(string objectId, string propertyType, string newValue)
    {
        // Реагируем только на изменения наших слотов
        if (propertyType != slotPropertyType) return;
        if (objectId != slotAObjectId && objectId != slotBObjectId) return;

        Recompute();
    }

    private void Recompute()
    {
        var ws = GameManager.Instance.World;
        string a = ws.GetString(slotAObjectId, slotPropertyType, defaultIfEmpty);
        string b = ws.GetString(slotBObjectId, slotPropertyType, defaultIfEmpty);

        string mixed = MixColors(a, b);

        Debug.Log($"[ColorMixer:{name}] a={a}, b={b}, mixed={mixed}, output→{outputObjectId}");

        // Применяем визуально к центральному квадрату
        if (centralSpriteRenderer != null)
            centralSpriteRenderer.color = StringToColor(mixed);

        // Записываем результат в WorldState (для HackableColor на 3D-кубе)
        if (!string.IsNullOrEmpty(outputObjectId))
            ws.SetString(outputObjectId, outputPropertyType, mixed);
    }

    /// <summary>Алгоритм смешивания цветов.</summary>
    public static string MixColors(string a, string b)
    {
        a = Normalize(a);
        b = Normalize(b);

        // Любая комбинация с "empty" → white
        if (a == "empty" || b == "empty") return "white";

        // Одинаковые → тот же цвет
        if (a == b) return a;

        // Сортируем для упрощения логики (red, blue, yellow по алфавиту)
        string first = a.CompareTo(b) < 0 ? a : b;
        string second = a.CompareTo(b) < 0 ? b : a;

        // blue + red = purple
        if (first == "blue" && second == "red") return "purple";
        // blue + yellow = green
        if (first == "blue" && second == "yellow") return "green";
        // red + yellow = orange
        if (first == "red" && second == "yellow") return "orange";

        // Неизвестная комбинация
        return "white";
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "empty";
        return s.Trim().ToLowerInvariant();
    }

    /// <summary>Преобразование строки цвета в Color (использует значения из инспектора).</summary>
    public Color StringToColor(string colorName)
    {
        switch (Normalize(colorName))
        {
            case "red":    return colorRed;
            case "blue":   return colorBlue;
            case "yellow": return colorYellow;
            case "purple": return colorPurple;
            case "orange": return colorOrange;
            case "green":  return colorGreen;
            case "white":
            default:       return colorWhite;
        }
    }
}
