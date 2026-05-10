using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроллер подбора объектов в стиле Half-Life.
/// </summary>
public class PickupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private float holdDistance = 1.5f;
    [SerializeField] private float maxFollowSpeed = 12f;
    [SerializeField] private float breakDistance = 3.5f;
    [SerializeField] private LayerMask raycastMask = ~0;

    [Header("Input")]
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

        if (held != null) Drop();
    }

    private void OnPickupPerformed(InputAction.CallbackContext ctx)
    {
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

        if (Physics.SphereCast(ray, 0.3f, out RaycastHit hit, pickupRange, raycastMask, QueryTriggerInteraction.Ignore))
        {
            Pickupable p = hit.collider.GetComponentInParent<Pickupable>();
            if (p != null)
            {
                held = p;
                held.OnPickup();

                // SFX
                SoundManager.Instance?.PlaySFX("PickUp");
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

        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * holdDistance;
        Vector3 toTarget = targetPos - held.transform.position;

        if (toTarget.magnitude > breakDistance)
        {
            Drop();
            return;
        }

        Vector3 desiredVelocity = toTarget * held.followStrength;

        if (desiredVelocity.magnitude > maxFollowSpeed)
            desiredVelocity = desiredVelocity.normalized * maxFollowSpeed;

        held.Rb.linearVelocity = desiredVelocity;
    }
}
