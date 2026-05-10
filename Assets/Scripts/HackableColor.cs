using UnityEngine;

/// <summary>
/// Меняет цвет 3D-куба на основе строкового свойства Color в WorldState.
/// 
/// Использует MaterialPropertyBlock — это значит, что мы НЕ создаём новый материал
/// (sharedMaterial остаётся общим, GPU-инстансинг продолжает работать).
///
/// Цветовые имена: "red", "blue", "yellow", "purple", "orange", "green", "white".
/// Палитра должна совпадать с ColorMixer (или настрой в инспекторе индивидуально).
/// </summary>
public class HackableColor : HackableStringProperty
{
    public override string PropertyType => "Color";

    [Header("Renderer")]
    [Tooltip("Renderer, чей цвет менять. Если null — берётся с этого GameObject или его детей.")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Shader Property")]
    [Tooltip("Имя свойства цвета в шейдере. Для URP/Lit это '_BaseColor'. Для Standard — '_Color'.")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Color Palette (можешь подправить, или скопировать из ColorMixer для согласованности)")]
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

        // По умолчанию value = "white"
        defaultValue = "white";
    }

    protected override void ApplyValue(string value)
    {
        Debug.Log($"[HackableColor on {name}, objId={owner?.objectId}] ApplyValue('{value}')"); 
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
