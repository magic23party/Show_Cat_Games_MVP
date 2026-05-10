using UnityEngine;

/// <summary>
/// Играет шаги в 3D. Вешается на 3D-игрока (с CharacterController).
/// Проверяет горизонтальную скорость и грунт, играет случайный звук с заданным интервалом.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepPlayer3D : MonoBehaviour
{
    [Tooltip("Интервал между шагами (сек). Меньше — быстрее.")]
    [SerializeField] private float stepInterval = 0.45f;

    [Tooltip("Минимальная горизонтальная скорость, при которой считаем что игрок 'идёт'.")]
    [SerializeField] private float minSpeed = 0.5f;

    private CharacterController cc;
    private float timer;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
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
        if (timer >= stepInterval)
        {
            timer = 0f;
            SoundManager.Instance?.PlayFootstep3D();
        }
    }
}
