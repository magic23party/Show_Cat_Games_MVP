using UnityEngine;

/// <summary>
/// Базовый класс для свойств со строковыми значениями (через HackSlot/HackCommand).
/// </summary>
[RequireComponent(typeof(HackableObject))]
public abstract class HackableStringProperty : MonoBehaviour
{
    protected HackableObject owner;
    public abstract string PropertyType { get; }

    [Tooltip("Значение по умолчанию, если в WorldState ничего нет.")]
    public string defaultValue = "true";

    protected virtual void Awake()
    {
        owner = GetComponent<HackableObject>();
    }

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            string currentValue = GameManager.Instance.World.GetString(owner.objectId, PropertyType, defaultValue);
            ApplyValue(currentValue);
            GameManager.Instance.World.OnStringChanged += HandleWorldChanged;
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.World.OnStringChanged -= HandleWorldChanged;
    }

    private void HandleWorldChanged(string objectId, string propertyType, string newValue)
    {
        if (objectId == owner.objectId && propertyType == PropertyType)
            ApplyValue(newValue);
    }

    protected abstract void ApplyValue(string value);

    protected static bool ParseAsBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        value = value.Trim().ToLowerInvariant();
        return value == "true" || value == "on" || value == "1" || value == "yes";
    }
}
