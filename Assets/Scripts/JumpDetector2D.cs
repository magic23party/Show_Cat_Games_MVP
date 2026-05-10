using UnityEngine;

/// <summary>
/// Определяет момент прыжка в 2D и играет звук Jump.
/// Вешается на 2D-игрока с Rigidbody2D.
///
/// Логика: если в прошлом кадре игрок был на земле, в этом нет, и y-скорость положительная — это прыжок.
/// Грунт определяется через Raycast вниз (если useRaycastGroundCheck) или через velocity.y ~ 0.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class JumpDetector2D : MonoBehaviour
{
    [Tooltip("Минимальная положительная вертикальная скорость, чтобы считать это прыжком.")]
    [SerializeField] private float minJumpVelocity = 1f;

    [Tooltip("Имя SFX в SoundManager.")]
    [SerializeField] private string jumpSfxName = "Jump";

    [Header("Ground Check")]
    [SerializeField] private bool useRaycastGroundCheck = false;
    [SerializeField] private LayerMask groundLayerMask = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;

    private Rigidbody2D rb;
    private bool wasGrounded = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        if (wasGrounded && !grounded && rb.linearVelocity.y > minJumpVelocity)
        {
            SoundManager.Instance?.PlaySFX(jumpSfxName);
        }

        wasGrounded = grounded;
    }

    private bool IsGrounded()
    {
        if (useRaycastGroundCheck)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayerMask);
            return hit.collider != null;
        }
        return Mathf.Abs(rb.linearVelocity.y) < 0.1f;
    }
}
