using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер подбора объектов в стиле Half-Life.
/// Вешается на 3D-игрока (или любой объект, у которого есть ссылка на камеру).
///
/// Логика:
/// - F (или назначенный Action) делает рейкаст из камеры.
/// - Если попал в Pickupable в радиусе pickupRange — подбирает.
/// - Удерживаемый ящик каждый FixedUpdate тянется физически к точке перед камерой.
/// - Повторное нажатие F — отпускает.
/// </summary>
public class PickupController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Камера от 1-го лица. Из неё пускается рейкаст и относительно неё считается точка удержания.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [Tooltip("Максимальная дальность подбора.")]
    [SerializeField] private float pickupRange = 2.5f;

    [Tooltip("Расстояние от камеры до точки, где висит ящик.")]
    [SerializeField] private float holdDistance = 1.5f;

    [Tooltip("Максимальная скорость, с которой ящик летит к руке. Защита от 'выстрела' тяжёлых объектов.")]
    [SerializeField] private float maxFollowSpeed = 12f;

    [Tooltip("Если ящик отлетел дальше этого расстояния — насильно отпускаем (например, его придавило стеной).")]
    [SerializeField] private float breakDistance = 3.5f;

    [Tooltip("Слои, которые рейкаст 'видит'. Поставь сюда Default + Pickupable. Игрока — НЕ ставь.")]
    [SerializeField] private LayerMask raycastMask = ~0;

    [Header("Input")]
    [Tooltip("InputAction для подбора/отпускания. Привяжи к F.")]
    [SerializeField] private InputActionReference pickupAction;

    private Pickupable held;

    private void OnEnable()
    {
        if (pickupAction != null)
        {
            pickupAction.action.Enable();
            pickupAction.action.performed += OnPickupPerformed;
        }
    }

    private void OnDisable()
    {
        if (pickupAction != null)
            pickupAction.action.performed -= OnPickupPerformed;

        // Если игрока выключили (например, при входе в баг) — отпускаем ящик принудительно
        if (held != null) Drop();
    }

    private void OnPickupPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[PickupController] F pressed! Held: " + (held != null));

        // Не разрешаем подбирать во время перехода в баг
        if (GameManager.Instance != null && GameManager.Instance.IsTransitioning) return;

        if (held != null)
            Drop();
        else
            TryPickup();
    }

    private void TryPickup()
    {
        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // SphereCast — луч с радиусом, ловит объекты в небольшой "трубе" вокруг прицела
        if (Physics.SphereCast(ray, 0.3f, out RaycastHit hit, pickupRange, raycastMask, QueryTriggerInteraction.Ignore))
        {
            Pickupable p = hit.collider.GetComponentInParent<Pickupable>();
            if (p != null)
            {
                held = p;
                held.OnPickup();
            }
        }
    }

    private void Drop()
    {
        if (held == null) return;
        held.OnDrop();
        held = null;
    }

    private void FixedUpdate()
    {
        if (held == null || cameraTransform == null) return;

        // Целевая точка — впереди камеры на расстоянии holdDistance
        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * holdDistance;
        Vector3 toTarget = targetPos - held.transform.position;

        // Если ящик улетел/застрял — отпускаем
        if (toTarget.magnitude > breakDistance)
        {
            Drop();
            return;
        }

        // Желаемая скорость пропорциональна расстоянию до цели
        Vector3 desiredVelocity = toTarget * held.followStrength;

        // Ограничиваем максимальную скорость, чтобы тяжёлые ящики не "выстреливали"
        if (desiredVelocity.magnitude > maxFollowSpeed)
            desiredVelocity = desiredVelocity.normalized * maxFollowSpeed;

        held.Rb.linearVelocity = desiredVelocity;
    }
}
