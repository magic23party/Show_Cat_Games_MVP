using UnityEngine;

/// <summary>
/// Слушает string-свойство в WorldState и переключает два GameObject (On/Off).
/// 
/// Используется для фоновых подсказок в 2D-сценах:
/// - Если значение = "true" → включается onObject, выключается offObject
/// - Иначе → наоборот
///
/// Не привязан к 3D-объекту через HackableObject — просто слушает WorldState напрямую.
/// </summary>
public class BackgroundFromBoolState : MonoBehaviour
{
    [Header("Source (откуда берём значение)")]
    [Tooltip("ID объекта в WorldState (тот же, что в HackableObject в 3D).")]
    [SerializeField] private string sourceObjectId;

    [Tooltip("Тип свойства, например 'Active'.")]
    [SerializeField] private string sourcePropertyType = "Active";

    [Tooltip("Значение по умолчанию, если в WorldState ничего не задано.")]
    [SerializeField] private string defaultValue = "false";

    [Header("Targets (что включаем/выключаем)")]
    [Tooltip("GameObject, активный когда значение = 'true'.")]
    [SerializeField] private GameObject onObject;

    [Tooltip("GameObject, активный когда значение != 'true'.")]
    [SerializeField] private GameObject offObject;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        // Применяем текущее значение
        string current = GameManager.Instance.World.GetString(sourceObjectId, sourcePropertyType, defaultValue);
        Apply(current);

        // Подписываемся на изменения
        GameManager.Instance.World.OnStringChanged += HandleChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.World.OnStringChanged -= HandleChanged;
    }

    private void HandleChanged(string objectId, string propertyType, string newValue)
    {
        if (objectId == sourceObjectId && propertyType == sourcePropertyType)
            Apply(newValue);
    }

    private void Apply(string value)
    {
        bool isOn = ParseAsBool(value);

        if (onObject != null) onObject.SetActive(isOn);
        if (offObject != null) offObject.SetActive(!isOn);
    }

    private static bool ParseAsBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        value = value.Trim().ToLowerInvariant();
        return value == "true" || value == "on" || value == "1" || value == "yes";
    }
}
