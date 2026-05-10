using UnityEngine;

/// <summary>
/// Управляет видимостью: включает/отключает все Renderer на объекте и его детях.
/// Это НЕ SetActive(false) — объект остаётся в сцене и его коллайдер работает (если коллизия не отключена).
/// Если хочешь "полное исчезновение" — добавь рядом и HackableCollision на тот же объект.
///
/// true = видим
/// false = невидим
/// </summary>
public class HackableVisibility : HackableProperty
{
    public override string PropertyType => "Visibility";

    [Tooltip("Если включено — управляет рендерерами на детях тоже.")]
    [SerializeField] private bool includeChildren = true;

    private Renderer[] renderers;

    protected override void Awake()
    {
        base.Awake();
        renderers = includeChildren ? GetComponentsInChildren<Renderer>(true) : GetComponents<Renderer>();
    }

    protected override void ApplyValue(bool value)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
            if (r != null) r.enabled = value;
    }
}
