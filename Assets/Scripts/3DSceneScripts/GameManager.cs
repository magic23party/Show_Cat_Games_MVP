using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Центральный менеджер игры. Синглтон с DontDestroyOnLoad.
///
/// Управление управлением игрока:
/// - При входе в баг: отключаем перед анимацией EnterBug, оставляем выключенным до возврата в 3D.
/// - При выходе из бага: включаем игрока, но управление выключено.
///   Включаем управление только ПОСЛЕ окончания анимации ExitBug + fade-in.
///
/// Анимации:
/// - EnterBug: triggered при входе в баг, ждём Animation Event в конце.
/// - ExitBug: triggered после выгрузки 2D, играется параллельно с fade-in.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player References (3D)")]
    [SerializeField] private GameObject player3D;
    [SerializeField] private GameObject playerHands;
    [SerializeField] private Animator handsAnimator;

    [Tooltip("Скрипты управления, которые отключаются на время анимации перехода.")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableDuringAnimation;

    [Header("UI References")]
    [SerializeField] private TransitionUI transitionUI;

    [Header("Animator Triggers")]
    [SerializeField] private string enterBugTriggerName = "EnterBug";
    [SerializeField] private string exitBugTriggerName = "ExitBug";

    public WorldState World { get; private set; }

    private bool isTransitioning = false;
    private BugData currentBug = null;
    private string currentSceneName = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        World = new WorldState();
    }

    private void Start()
    {
        LockCursor();
        if (playerHands != null) playerHands.SetActive(false);
    }

    public void EnterBug(BugData data)
    {
        if (isTransitioning) return;
        if (data == null) { Debug.LogError("[GameManager] BugData is null!"); return; }

        isTransitioning = true;
        StartCoroutine(EnterBugRoutine(data));
    }

    public void ExitBug()
    {
        if (isTransitioning) return;
        if (currentBug == null) return;

        isTransitioning = true;
        StartCoroutine(ExitBugRoutine());
    }

    private IEnumerator EnterBugRoutine(BugData data)
    {
        currentBug = data;

        // Отключаем управление
        SetPlayerControlEnabled(false);

        // Включаем руки и запускаем EnterBug
        if (playerHands != null) playerHands.SetActive(true);
        if (handsAnimator != null)
        {
            // Сбрасываем Animator чтобы анимация гарантированно проигралась с начала
            handsAnimator.Rebind();
            handsAnimator.Update(0f);
            handsAnimator.SetTrigger(enterBugTriggerName);
        }

        // Ждём Animation Event
        yield return new WaitUntil(() => animationFinishedFlag);
        animationFinishedFlag = false;

        // Fade-to-black
        yield return transitionUI.FadeOut();

        // Прячем руки и 3D
        if (playerHands != null) playerHands.SetActive(false);
        if (player3D != null) player3D.SetActive(false);

        // Грузим 2D
        currentSceneName = data.sceneName;
        AsyncOperation load = SceneManager.LoadSceneAsync(currentSceneName, LoadSceneMode.Additive);
        yield return load;

        Scene loadedScene = SceneManager.GetSceneByName(currentSceneName);
        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);

        // Fade-in
        yield return transitionUI.FadeIn();

        isTransitioning = false;
    }

    private IEnumerator ExitBugRoutine()
    {
        // Fade-to-black
        yield return transitionUI.FadeOut();

        // Выгружаем 2D
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentSceneName);
            yield return unload;
            currentSceneName = null;
        }

        currentBug = null;

        // Включаем 3D-игрока (но управление ВЫКЛ)
        if (player3D != null) player3D.SetActive(true);
        SetPlayerControlEnabled(false);

        LockCursor();

        // Включаем руки и запускаем ExitBug
        if (playerHands != null) playerHands.SetActive(true);
        if (handsAnimator != null)
        {
            // Сбрасываем Animator чтобы анимация ExitBug гарантированно проигралась с начала
            handsAnimator.Rebind();
            handsAnimator.Update(0f);
            handsAnimator.SetTrigger(exitBugTriggerName);
        }

        // ПАРАЛЛЕЛЬНО: fade-in и анимация ExitBug
        animationFinishedFlag = false;
        Coroutine fadeInRoutine = StartCoroutine(transitionUI.FadeIn());
        Coroutine waitAnimRoutine = StartCoroutine(WaitForAnimationFinished());

        // Ждём оба
        yield return fadeInRoutine;
        yield return waitAnimRoutine;

        // Прячем руки
        if (playerHands != null) playerHands.SetActive(false);

        // Включаем управление — теперь игрок может всё, в том числе нажать E
        SetPlayerControlEnabled(true);

        isTransitioning = false;
    }

    private IEnumerator WaitForAnimationFinished()
    {
        yield return new WaitUntil(() => animationFinishedFlag);
        animationFinishedFlag = false;
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (scriptsToDisableDuringAnimation == null) return;
        foreach (var script in scriptsToDisableDuringAnimation)
            if (script != null) script.enabled = enabled;
    }

    // === Animation Event Bridge ===
    private bool animationFinishedFlag = false;
    public void NotifyAnimationFinished() => animationFinishedFlag = true;

    // === Cursor ===
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsTransitioning => isTransitioning;
}
