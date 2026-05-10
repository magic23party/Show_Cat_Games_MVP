using UnityEngine;
using AZE.AdvancedFirstPerson;

/// <summary>
/// Играет шаги в 3D. Вешается на 3D-игрока (с CharacterController).
/// Интервал шага и громкость зависят от скорости и от того, присел ли игрок:
///   — спринт:  чаще, обычная громкость
///   — ходьба:  стандартный интервал
///   — присед:  реже, тише (sneaky steps)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepPlayer3D : MonoBehaviour
{
    [Header("Step Timing")]
    [Tooltip("Интервал между шагами при обычной ходьбе (сек).")]
    [SerializeField] private float walkStepInterval = 0.45f;

    [Tooltip("Интервал при спринте (сек).")]
    [SerializeField] private float sprintStepInterval = 0.30f;

    [Tooltip("Интервал при приседании (сек).")]
    [SerializeField] private float crouchStepInterval = 0.55f;

    [Header("Volume")]
    [Tooltip("Множитель громкости в приседе.")]
    [Range(0f, 1f)]
    [SerializeField] private float crouchVolumeMul = 0.45f;

    [Header("Detection")]
    [Tooltip("Минимальная горизонтальная скорость, при которой считаем что игрок 'идёт'.")]
    [SerializeField] private float minSpeed = 0.5f;

    [Tooltip("Порог CurrentSpeedPercentage, выше которого считаем что это спринт. " +
             "Walk даёт ≈ WalkSpeed/RunSpeed (например 4.5/7 = 0.64), поэтому порог должен быть выше.")]
    [Range(0f, 1f)]
    [SerializeField] private float sprintThreshold = 0.85f;

    private CharacterController cc;
    private PlayerMovementStateMachine movement;
    private float timer;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovementStateMachine>();
    }

    private void Update()
    {
        Vector3 v = cc.velocity;
        v.y = 0f;
        bool moving = v.magnitude > minSpeed && cc.isGrounded;

        if (!moving)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        float interval = walkStepInterval;
        float volumeMul = 1f;

        if (movement != null)
        {
            bool isCrouching = cc.height < movement.GetStandingHeight() - 0.1f;
            float speedPct = movement.CurrentSpeedPercentage;

            if (isCrouching)
            {
                interval = crouchStepInterval;
                volumeMul = crouchVolumeMul;
            }
            else if (speedPct >= sprintThreshold)
            {
                interval = sprintStepInterval;
            }
            else
            {
                interval = walkStepInterval;
            }
        }

        if (timer >= interval)
        {
            timer = 0f;
            SoundManager.Instance?.PlayFootstep3D(volumeMul);
        }
    }
}
