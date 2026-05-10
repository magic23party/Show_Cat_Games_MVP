using UnityEngine;
using UltimateCC;

/// <summary>
/// Простой sprite-animator для 2D-игрока. Полностью байпасит .anim-кривые
/// (которые анимируют только m_Color) и подменяет спрайт сам, читая
/// PlayerMain.CurrentState каждый кадр.
///
/// Зачем: оригинальные .anim файлы не содержат кадров спрайтов
/// (m_PPtrCurves пустой) — поэтому Animator один цвет туда-сюда крутит,
/// но кадры не меняются. Этот скрипт делает то, что должен был
/// делать аниматор.
///
/// Как использовать:
///   1) Положи скрипт на GameObject 2D-игрока (тот, у которого PlayerMain
///      и SpriteRenderer тела).
///   2) В инспекторе закинь спрайты Idle_1..5 / walk_1..5 / Run_1..5 / Jump_1..5
///      в соответствующие массивы (или они уже подставлены в префабе).
///   3) Опционально: задай framesPerSecond и runSpeedThreshold.
/// </summary>
public class SimplePlayerSpriteAnimator : MonoBehaviour
{
    [Header("Renderer")]
    [Tooltip("SpriteRenderer тела игрока. Если пусто — берётся GetComponent<SpriteRenderer>() с этого же объекта.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprite Frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] runFrames;
    [SerializeField] private Sprite[] jumpFrames;

    [Header("Settings")]
    [Tooltip("Скорость воспроизведения кадров (кадров в секунду).")]
    [SerializeField] private float framesPerSecond = 10f;

    [Tooltip("|velocity.x| выше этого порога считается бегом (Run vs Walk).")]
    [SerializeField] private float runSpeedThreshold = 5f;

    [Tooltip("Если true — спрайт зеркалится по X в зависимости от направления движения.")]
    [SerializeField] private bool flipBasedOnMovement = true;

    [Tooltip("Если true — каждый кадр сбрасываем m_Color до белого, чтобы Animator цветовыми кривыми не затемнял новые спрайты.")]
    [SerializeField] private bool overrideAnimatorColor = true;

    private PlayerMain playerMain;
    private Rigidbody2D rb;

    private float frameTimer;
    private int frameIndex;
    private Sprite[] currentSet;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        playerMain = GetComponent<PlayerMain>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null) return;

        Sprite[] target = ChooseFrameSet();

        if (target != currentSet)
        {
            currentSet = target;
            frameIndex = 0;
            frameTimer = 0f;
        }

        if (currentSet != null && currentSet.Length > 0)
        {
            frameTimer += Time.deltaTime;
            float frameDur = 1f / Mathf.Max(0.01f, framesPerSecond);
            if (frameTimer >= frameDur)
            {
                frameTimer -= frameDur;
                frameIndex = (frameIndex + 1) % currentSet.Length;
            }

            Sprite frame = currentSet[frameIndex];
            if (frame != null) spriteRenderer.sprite = frame;
        }

        // Аниматор анимирует m_Color (затемняет в серый) — пересиливаем белым.
        if (overrideAnimatorColor)
            spriteRenderer.color = Color.white;

        if (flipBasedOnMovement && rb != null)
        {
            float vx = rb.linearVelocity.x;
            if (Mathf.Abs(vx) > 0.1f)
                spriteRenderer.flipX = vx < 0f;
        }
    }

    private Sprite[] ChooseFrameSet()
    {
        if (playerMain == null) return idleFrames;

        switch (playerMain.CurrentState)
        {
            case PlayerMain.AnimName.Jump:
            case PlayerMain.AnimName.ExtraJump1:
            case PlayerMain.AnimName.ExtraJump2:
            case PlayerMain.AnimName.Land:
            case PlayerMain.AnimName.WallJump:
            case PlayerMain.AnimName.WallSlide:
                return PickNonEmpty(jumpFrames, idleFrames);

            case PlayerMain.AnimName.Walk:
            case PlayerMain.AnimName.Dash:
                if (rb != null
                    && Mathf.Abs(rb.linearVelocity.x) >= runSpeedThreshold
                    && runFrames != null && runFrames.Length > 0)
                    return runFrames;
                return PickNonEmpty(walkFrames, idleFrames);

            case PlayerMain.AnimName.Idle:
            case PlayerMain.AnimName.CrouchIdle:
            case PlayerMain.AnimName.CrouchWalk:
            case PlayerMain.AnimName.WallGrab:
            case PlayerMain.AnimName.WallClimb:
            default:
                return idleFrames;
        }
    }

    private static Sprite[] PickNonEmpty(Sprite[] primary, Sprite[] fallback)
    {
        return (primary != null && primary.Length > 0) ? primary : fallback;
    }
}
