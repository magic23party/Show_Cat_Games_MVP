using UnityEngine;

/// <summary>
/// Маркер для объектов, которые можно подобрать.
/// Вешается на ящики (или другие физические объекты).
/// Требует Rigidbody.
/// Также рекомендуется поставить Layer = Pickupable для оптимизации рейкаста.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    [Tooltip("На сколько силён 'магнит', который тянет ящик к руке. Больше = резче следует за камерой.")]
    public float followStrength = 20f;

    [Tooltip("Угловое демпфирование во время удержания (чтобы ящик не крутился безумно).")]
    public float heldAngularDamping = 10f;

    // Сохранённые настройки, чтобы восстановить при отпускании
    private float originalAngularDamping;
    private float originalLinearDamping;
    private bool originalUseGravity;
    private RigidbodyInterpolation originalInterpolation;
    private CollisionDetectionMode originalCollisionMode;

    private Rigidbody rb;
    public Rigidbody Rb => rb;

    public bool IsHeld { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>Вызывается из PickupController при подборе.</summary>
    public void OnPickup()
    {
        IsHeld = true;

        // Сохраняем оригинальные настройки
        originalAngularDamping = rb.angularDamping;
        originalLinearDamping = rb.linearDamping;
        originalUseGravity = rb.useGravity;
        originalInterpolation = rb.interpolation;
        originalCollisionMode = rb.collisionDetectionMode;

        // Настройки во время удержания
        rb.useGravity = false;                                  // не тянет вниз
        rb.angularDamping = heldAngularDamping;                 // меньше вращения
        rb.linearDamping = 10f;                                 // плавная остановка
        rb.interpolation = RigidbodyInterpolation.Interpolate;  // плавный визуал
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // не пролетит сквозь стены
    }

    /// <summary>Вызывается из PickupController при отпускании.</summary>
    public void OnDrop()
    {
        IsHeld = false;

        // Возвращаем оригинальные настройки
        rb.useGravity = originalUseGravity;
        rb.angularDamping = originalAngularDamping;
        rb.linearDamping = originalLinearDamping;
        rb.interpolation = originalInterpolation;
        rb.collisionDetectionMode = originalCollisionMode;
    }
}
