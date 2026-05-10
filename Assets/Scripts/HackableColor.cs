using UnityEngine;

/// <summary>
/// Меняет цвет 3D-куба на основе строкового свойства Color в WorldState.
/// Использует MaterialPropertyBlock — не создаёт копий материала.
///
/// Реагирует только на события OnStringChanged + применяет initialColor при старте.
/// </summary>
public class HackableColor : HackableStringProperty
{
    public override string PropertyType => "Color";

    [Header("Initial Color (стартовый цвет, перезаписывает WorldState при загрузке сцены)")]
    [SerializeField] private string initialColor = "white";

    [Header("Renderer")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Shader Property")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Color Palette")]
    [SerializeField] private Color colorRed = new Color(0.9f, 0.15f, 0.15f);
    [SerializeField] private Color colorBlue = new Color(0.15f, 0.3f, 0.9f);
    [SerializeField] private Color colorYellow = new Color(1f, 0.9f, 0.1f);
    [SerializeField] private Color colorPurple = new Color(0.6f, 0.2f, 0.8f);
    [SerializeField] private Color colorOrange = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private Color colorGreen = new Color(0.2f, 0.75f, 0.25f);
    [SerializeField] private Color colorWhite = Color.white;

    private MaterialPropertyBlock mpb;
    private int colorPropertyId;

    protected override void Awake()
    {
        base.Awake();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        mpb = new MaterialPropertyBlock();
        colorPropertyId = Shader.PropertyToID(colorPropertyName);

        defaultValue = initialColor;
    }

    protected override void Start()
    {
        // Принудительно записываем initialColor при старте — куб всегда стартует в нужном цвете
        if (GameManager.Instance != null && !string.IsNullOrEmpty(initialColor))
        {
            GameManager.Instance.World.SetString(owner.objectId, PropertyType, initialColor);
        }

        base.Start();
    }

    protected override void ApplyValue(string value)
    {
        if (targetRenderer == null) return;

        Color c = StringToColor(value);

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorPropertyId, c);
        targetRenderer.SetPropertyBlock(mpb);
    }

    public Color StringToColor(string colorName)
    {
        if (string.IsNullOrEmpty(colorName)) return colorWhite;
        switch (colorName.Trim().ToLowerInvariant())
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
