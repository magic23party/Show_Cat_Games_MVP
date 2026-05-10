using UnityEngine;

/// <summary>
/// Играет шаги в 2D. Вешается на 2D-игрока (с Rigidbody2D).
/// 
/// Грунт определяется через короткий рейкаст вниз — если поставишь groundLayerMask
/// и groundCheckDistance, будет точно. Иначе считаем "на земле" если |velocity.y| < 0.1.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FootstepPlayer2D : MonoBehaviour
{
    [Tooltip("Интервал между шагами (сек).")]
    [SerializeField] private float stepInterval = 0.4f;

    [Tooltip("Минимальная горизонтальная скорость для 'идёт'.")]
    [SerializeField] private float minSpeed = 0.5f;

    [Header("Ground Check (опционально, для точности)")]
    [Tooltip("Если задано — используется рейкаст для определения 'на земле'. Иначе fallback на velocity.y.")]
    [SerializeField] private LayerMask groundLayerMask = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private bool useRaycastGroundCheck = false;

    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        bool grounded = IsGrounded();
        bool moving = Mathf.Abs(rb.linearVelocity.x) > minSpeed && grounded;

        if (!moving)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0f;
            SoundManager.Instance?.PlayFootstep2D();
        }
    }

    private bool IsGrounded()
    {
        if (useRaycastGroundCheck)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayerMask);
            return hit.collider != null;
        }
        // Fallback: считаем "на земле" если вертикальная скорость почти ноль
        return Mathf.Abs(rb.linearVelocity.y) < 0.1f;
    }
}
