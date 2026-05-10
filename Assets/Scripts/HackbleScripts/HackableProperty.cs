using UnityEngine;

/// <summary>
/// Базовый класс для bool-свойств (Collision, Gravity, Visibility).
/// Для int-свойств (Scale, Rotation) есть отдельный компонент HackableTransform.
/// </summary>
[RequireComponent(typeof(HackableObject))]
public abstract class HackableProperty : MonoBehaviour
{
    protected HackableObject owner;

    public abstract string PropertyType { get; }

    protected virtual void Awake()
    {
        owner = GetComponent<HackableObject>();
    }

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            bool currentValue = GameManager.Instance.World.Get(owner.objectId, PropertyType);
            ApplyValue(currentValue);

            GameManager.Instance.World.OnBoolChanged += HandleWorldChanged;
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.World.OnBoolChanged -= HandleWorldChanged;
    }

    private void HandleWorldChanged(string objectId, string propertyType, bool newValue)
    {
        if (objectId == owner.objectId && propertyType == PropertyType)
            ApplyValue(newValue);
    }

    protected abstract void ApplyValue(bool value);
}
