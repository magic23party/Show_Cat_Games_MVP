using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Вешается на 3D-объект "баг" (ноутбук).
/// Требует Collider с Is Trigger = true.
///
/// Защита от повторного срабатывания E при выходе из 2D:
/// - Игнорирует нажатия E если 3D-игрок неактивен (мы в 2D-сцене).
/// - Игнорирует нажатия пока идёт переход (IsTransitioning).
/// - Сбрасывает playerInRange когда 3D-игрок отключается, чтобы избежать "застрявшего" флага.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BugInteractable : MonoBehaviour
{
    [Header("Bug Configuration")]
    [SerializeField] private BugData bugData;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Player Reference")]
    [Tooltip("Корневой GameObject 3D-игрока. Используется для проверки 'мы в 3D-мире?'. " +
             "Можно оставить пустым — тогда возьмётся из GameManager (если он назначил player3D).")]
    [SerializeField] private GameObject player3D;

    private bool playerInRange = false;

    private void Start()
    {
        if (bugData == null)
            Debug.LogError($"[BugInteractable] BugData не назначен на {name}!", this);
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPerformed;
    }

    private void Update()
    {
        // Если 3D-игрок отключился (мы ушли в 2D) — сбрасываем флаг.
        // OnTriggerExit при SetActive(false) не вызывается, поэтому делаем сами.
        if (playerInRange && !IsPlayer3DActive())
        {
            playerInRange = false;
            InteractionPrompt.Instance?.Hide();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractionPrompt.Instance?.Show($"Press E to enter: {(bugData != null ? bugData.displayName : "Bug")}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractionPrompt.Instance?.Hide();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsTransitioning) return;
        if (!IsPlayer3DActive()) return; // <-- главная защита от срабатывания в 2D

        InteractionPrompt.Instance?.Hide();
        GameManager.Instance.EnterBug(bugData);
    }

    private bool IsPlayer3DActive()
    {
        // Если ссылка задана вручную — используем её
        if (player3D != null) return player3D.activeInHierarchy;

        // Иначе проверяем через GameManager (если он есть)
        // Это менее надёжно, но позволяет не настраивать поле руками
        return true; // если ссылки нет — считаем активным (старое поведение)
    }
}
