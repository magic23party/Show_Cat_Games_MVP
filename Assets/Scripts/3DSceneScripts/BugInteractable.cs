using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Вешается на 3D-объект "баг" (ноутбук).
/// Требует Collider с Is Trigger = true.
///
/// Защита от повторного срабатывания E при выходе из 2D:
/// - Игнорирует нажатия E если игрок уже внутри какого-либо 2D-бага (GameManager.IsInBug).
/// - Игнорирует нажатия пока идёт переход (IsTransitioning).
/// - Сбрасывает playerInRange когда мы заходим в 2D, чтобы избежать "застрявшего" флага
///   (OnTriggerExit не срабатывает при SetActive(false) на игроке).
/// </summary>
[RequireComponent(typeof(Collider))]
public class BugInteractable : MonoBehaviour
{
    [Header("Bug Configuration")]
    [SerializeField] private BugData bugData;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

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
        // Если мы уже внутри 2D-бага — сбрасываем флаг и прячем подсказку.
        // OnTriggerExit при SetActive(false) на игроке не вызывается, поэтому делаем сами.
        if (playerInRange && GameManager.Instance != null && GameManager.Instance.IsInBug)
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
        if (GameManager.Instance.IsInBug) return; // главная защита от срабатывания E из 2D

        InteractionPrompt.Instance?.Hide();
        GameManager.Instance.EnterBug(bugData);
    }
}
