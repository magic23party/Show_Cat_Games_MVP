using UnityEngine;

/// <summary>
/// Управляет гравитацией Rigidbody.
/// true = гравитация работает (нормально)
/// false = гравитация отключена (объект "висит" в воздухе)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HackableGravity : HackableProperty
{
    public override string PropertyType => "Gravity";

    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    protected override void ApplyValue(bool value)
    {
        if (rb == null) return;
        rb.useGravity = value;

        // Если гравитацию отключили — обнуляем вертикальную скорость, чтобы "застыл"
        if (!value)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0;
            rb.linearVelocity = v;
        }
    }
}
