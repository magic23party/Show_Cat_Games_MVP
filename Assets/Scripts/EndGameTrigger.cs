using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Триггер окончания игры.
/// При касании игрока: fade-to-black с настраиваемой длительностью,
/// затем задержка на чёрном экране, затем загрузка сцены меню.
///
/// Использует свой собственный CanvasGroup для fade — не зависит от TransitionUI.
/// </summary>
public class EndGameTrigger : MonoBehaviour
{
    public enum TriggerMode { ThreeD, TwoD }

    [Header("Trigger Mode")]
    [SerializeField] private TriggerMode mode = TriggerMode.ThreeD;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Scene")]
    [Tooltip("Имя сцены меню (как в Build Settings).")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Timings")]
    [Tooltip("Сколько секунд длится затемнение экрана.")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Сколько секунд ждать на чёрном экране перед загрузкой меню.")]
    [SerializeField] private float delayBeforeMenu = 1f;

    [Header("Fade UI")]
    [Tooltip("CanvasGroup чёрной панели на весь экран. Если оставить null — попробуем найти TransitionUI в сцене и использовать его CanvasGroup.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (mode != TriggerMode.ThreeD) return;
        if (!other.CompareTag(playerTag)) return;
        Trigger();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mode != TriggerMode.TwoD) return;
        if (!other.CompareTag(playerTag)) return;
        Trigger();
    }

    private void Trigger()
    {
        if (fired) return;
        fired = true;
        StartCoroutine(EndGameRoutine());
    }

    private IEnumerator EndGameRoutine()
    {
        CanvasGroup cg = ResolveCanvasGroup();

        // Fade-to-black
        if (cg != null)
            yield return FadeRoutine(cg, 0f, 1f, fadeDuration);
        else
            yield return new WaitForSecondsRealtime(fadeDuration); // fallback

        // Задержка на чёрном
        yield return new WaitForSecondsRealtime(delayBeforeMenu);

        // Курсор для меню
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Загрузка меню
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }

    private CanvasGroup ResolveCanvasGroup()
    {
        if (fadeCanvasGroup != null) return fadeCanvasGroup;

        // Запасной вариант — найти TransitionUI в сцене и взять его CanvasGroup
        var transition = FindAnyObjectByType<TransitionUI>();
        if (transition != null)
            return transition.GetComponent<CanvasGroup>();

        return null;
    }

    private IEnumerator FadeRoutine(CanvasGroup cg, float from, float to, float duration)
    {
        cg.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }
}
