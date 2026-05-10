using UnityEngine;

/// <summary>
/// Полностью включает/выключает дочерний GameObject (visualTarget).
/// Применяет строковое значение: "true" = активен, остальное = неактивен.
/// </summary>
public class HackableActive : HackableStringProperty
{
    public override string PropertyType => "Active";

    [Tooltip("ДОЧЕРНИЙ GameObject, который мы включаем/выключаем. НЕ сам объект — иначе скрипт умрёт.")]
    [SerializeField] private GameObject visualTarget;

    protected override void Awake()
    {
        base.Awake();
        if (visualTarget == null)
            Debug.LogWarning($"[HackableActive] visualTarget не задан на {name}!", this);
    }

    protected override void ApplyValue(string value)
    {
        if (visualTarget == null) return;
        visualTarget.SetActive(ParseAsBool(value));
    }
}
