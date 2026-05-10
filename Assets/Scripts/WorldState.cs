using System.Collections.Generic;

/// <summary>
/// Глобальное хранилище состояний всех "хакабельных" свойств.
///
/// Поддерживает три типа значений:
/// - bool: для переключателей (Collision, Gravity, Visibility)
/// - int:  для числовых параметров (Scale.X, Rotation.Y и т.д.)
/// - string: для команд (Active, и в будущем — произвольные слова)
///
/// Ключ: objectId + ":" + propertyType
/// </summary>
public class WorldState
{
    private readonly Dictionary<string, bool> boolValues = new Dictionary<string, bool>();
    private readonly Dictionary<string, int> intValues = new Dictionary<string, int>();
    private readonly Dictionary<string, string> stringValues = new Dictionary<string, string>();

    public System.Action<string, string, bool> OnBoolChanged;
    public System.Action<string, string, int> OnIntChanged;
    public System.Action<string, string, string> OnStringChanged;

    private string Key(string objectId, string propertyType) => $"{objectId}:{propertyType}";

    // === BOOL ===

    public bool Get(string objectId, string propertyType)
    {
        return !boolValues.TryGetValue(Key(objectId, propertyType), out bool value) || value;
    }

    public void Set(string objectId, string propertyType, bool value)
    {
        string key = Key(objectId, propertyType);
        bool oldValue = !boolValues.TryGetValue(key, out bool v) || v;

        boolValues[key] = value;

        if (oldValue != value)
            OnBoolChanged?.Invoke(objectId, propertyType, value);
    }

    public bool Toggle(string objectId, string propertyType)
    {
        bool current = Get(objectId, propertyType);
        Set(objectId, propertyType, !current);
        return !current;
    }

    // === INT ===

    public int GetInt(string objectId, string propertyType, int defaultValue)
    {
        return intValues.TryGetValue(Key(objectId, propertyType), out int value) ? value : defaultValue;
    }

    public void SetInt(string objectId, string propertyType, int value)
    {
        string key = Key(objectId, propertyType);
        bool hadValue = intValues.TryGetValue(key, out int oldValue);

        intValues[key] = value;

        if (!hadValue || oldValue != value)
            OnIntChanged?.Invoke(objectId, propertyType, value);
    }

    // === STRING ===

    /// <summary>Получить string-значение. Если не задано — возвращает defaultValue.</summary>
    public string GetString(string objectId, string propertyType, string defaultValue)
    {
        return stringValues.TryGetValue(Key(objectId, propertyType), out string value) ? value : defaultValue;
    }

    /// <summary>Задать string-значение. Триггерит OnStringChanged.</summary>
    public void SetString(string objectId, string propertyType, string value)
    {
        string key = Key(objectId, propertyType);
        bool hadValue = stringValues.TryGetValue(key, out string oldValue);

        stringValues[key] = value;

        if (!hadValue || oldValue != value)
            OnStringChanged?.Invoke(objectId, propertyType, value);
    }
}
