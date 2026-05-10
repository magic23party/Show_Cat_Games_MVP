using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Центральный менеджер игры. Синглтон с DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player References (3D)")]
    [SerializeField] private GameObject player3D;
    [SerializeField] private GameObject playerHands;
    [SerializeField] private Animator handsAnimator;

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

        SetPlayerControlEnabled(false);

        // SFX: вход в 2D-мир
        SoundManager.Instance?.PlaySFX("TurnOn");

        if (playerHands != null) playerHands.SetActive(true);
        if (handsAnimator != null)
        {
            handsAnimator.Rebind();
            handsAnimator.Update(0f);
            handsAnimator.SetTrigger(enterBugTriggerName);
        }

        yield return new WaitUntil(() => animationFinishedFlag);
        animationFinishedFlag = false;

        yield return transitionUI.FadeOut();

        if (playerHands != null) playerHands.SetActive(false);
        if (player3D != null) player3D.SetActive(false);

        currentSceneName = data.sceneName;
        AsyncOperation load = SceneManager.LoadSceneAsync(currentSceneName, LoadSceneMode.Additive);
        yield return load;

        Scene loadedScene = SceneManager.GetSceneByName(currentSceneName);
        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);

        // Музыка: переключаем на 2D
        SoundManager.Instance?.PlayMusic2D();

        yield return transitionUI.FadeIn();

        isTransitioning = false;
    }

    private IEnumerator ExitBugRoutine()
    {
        // SFX: выход из 2D
        SoundManager.Instance?.PlaySFX("TurnOff");

        yield return transitionUI.FadeOut();

        if (!string.IsNullOrEmpty(currentSceneName))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentSceneName);
            yield return unload;
            currentSceneName = null;
        }

        currentBug = null;

        if (player3D != null) player3D.SetActive(true);
        SetPlayerControlEnabled(false);

        LockCursor();

        // Музыка: переключаем обратно на 3D
        SoundManager.Instance?.PlayMusic3D();

        if (playerHands != null) playerHands.SetActive(true);
        if (handsAnimator != null)
        {
            handsAnimator.Rebind();
            handsAnimator.Update(0f);
            handsAnimator.SetTrigger(exitBugTriggerName);
        }

        animationFinishedFlag = false;
        Coroutine fadeInRoutine = StartCoroutine(transitionUI.FadeIn());
        Coroutine waitAnimRoutine = StartCoroutine(WaitForAnimationFinished());

        yield return fadeInRoutine;
        yield return waitAnimRoutine;

        if (playerHands != null) playerHands.SetActive(false);

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

    private bool animationFinishedFlag = false;
    public void NotifyAnimationFinished() => animationFinishedFlag = true;

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsTransitioning => isTransitioning;
}
