using UnityEngine;

/// <summary>
/// Управляет коллизией: включает/отключает все Collider-компоненты на объекте и его детях.
/// true = коллизия есть (нормально)
/// false = коллизия отключена (можно пройти сквозь)
/// </summary>
public class HackableCollision : HackableProperty
{
    public override string PropertyType => "Collision";

    [Tooltip("Если включено — управляет коллайдерами на детях тоже. Обычно нужно true.")]
    [SerializeField] private bool includeChildren = true;

    private Collider[] colliders;

    protected override void Awake()
    {
        base.Awake();
        colliders = includeChildren ? GetComponentsInChildren<Collider>(true) : GetComponents<Collider>();
    }

    protected override void ApplyValue(bool value)
    {
        if (colliders == null) return;
        foreach (var c in colliders)
            if (c != null) c.enabled = value;
    }
}
