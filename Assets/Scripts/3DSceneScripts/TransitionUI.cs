using System.Collections;
using UnityEngine;

/// <summary>
/// Управляет fade-переходом через CanvasGroup.
/// Вешается на Canvas с чёрной полупрозрачной панелью (Image, alpha=1, фон чёрный).
/// На корневом объекте Canvas нужен компонент CanvasGroup.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class TransitionUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Стартуем с прозрачного экрана
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>Затемнение в чёрный.</summary>
    public IEnumerator FadeOut() => Fade(0f, 1f);

    /// <summary>Просветление от чёрного.</summary>
    public IEnumerator FadeIn() => Fade(1f, 0f);

    private IEnumerator Fade(float from, float to)
    {
        canvasGroup.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled — на случай если кто-то ставит Time.timeScale = 0
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        canvasGroup.alpha = to;
        canvasGroup.blocksRaycasts = (to > 0f);
    }
}
