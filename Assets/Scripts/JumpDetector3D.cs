using UnityEngine;

/// <summary>
/// Определяет момент прыжка в 3D и играет звук Jump.
/// Вешается на 3D-игрока с CharacterController.
///
/// Логика: если в прошлом кадре игрок был на земле, а в этом — нет, и y-скорость положительная — это прыжок.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class JumpDetector3D : MonoBehaviour
{
    [Tooltip("Минимальная положительная вертикальная скорость, чтобы считать это прыжком (а не падением).")]
    [SerializeField] private float minJumpVelocity = 0.5f;

    [Tooltip("Имя SFX в SoundManager.")]
    [SerializeField] private string jumpSfxName = "Jump";

    private CharacterController cc;
    private bool wasGrounded = true;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        bool grounded = cc.isGrounded;

        // Был на земле, а сейчас нет, и движемся вверх — прыжок
        if (wasGrounded && !grounded && cc.velocity.y > minJumpVelocity)
        {
            SoundManager.Instance?.PlaySFX(jumpSfxName);
        }

        wasGrounded = grounded;
    }
}
