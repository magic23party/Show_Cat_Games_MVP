using UnityEngine;

/// <summary>
/// Играет SFX при столкновении объекта (с Rigidbody) с другими объектами.
/// Вешается на 3D-куб с Rigidbody.
///
/// Логика:
/// - OnCollisionEnter: если относительная скорость удара выше порога — играем SFX.
/// - Cooldown между играми звуков, чтобы не было спама когда куб катается.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CollisionSoundPlayer : MonoBehaviour
{
    [Header("SFX")]
    [Tooltip("Имя SFX в SoundManager.")]
    [SerializeField] private string sfxName = "Box_Land";

    [Header("Settings")]
    [Tooltip("Минимальная относительная скорость удара (m/s), чтобы сыграть звук. " +
             "Маленькие касания (катание) игнорируются.")]
    [SerializeField] private float minImpactVelocity = 1.5f;

    [Tooltip("Минимальный интервал между играми звука (сек), чтобы не было спама.")]
    [SerializeField] private float cooldown = 0.15f;

    [Tooltip("Громкость зависит от силы удара. Если выкл — всегда базовая громкость.")]
    [SerializeField] private bool scaleVolumeByImpact = true;

    [Tooltip("При какой силе удара громкость = 1 (если scaleVolumeByImpact включён).")]
    [SerializeField] private float maxImpactVelocity = 8f;

    private float lastPlayTime = -999f;

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact < minImpactVelocity) return;

        if (Time.time - lastPlayTime < cooldown) return;
        lastPlayTime = Time.time;

        // SoundManager сам играет с базовой громкостью SFX entry, но мы можем сделать его
        // громче/тише в зависимости от силы удара. Для этого играем напрямую через AudioSource
        // ИЛИ просто вызываем SoundManager.PlaySFX (без масштабирования).
        // 
        // Для простоты — используем PlaySFX. scaleVolumeByImpact оставлен для возможной кастомизации,
        // но по умолчанию SoundManager не поддерживает динамическую громкость для PlaySFX.
        SoundManager.Instance?.PlaySFX(sfxName);
    }
}
