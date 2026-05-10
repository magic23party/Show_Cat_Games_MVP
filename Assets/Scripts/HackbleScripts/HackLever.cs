using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Рычаг в 2D-сцене. При взаимодействии переключает указанное BOOL-свойство в WorldState.
/// Для числовых параметров (Scale, Rotation) используй HackTransformControl.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackLever : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetObjectId;
    [SerializeField] private string targetPropertyType = "Collision";

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteOff;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("UI Hint")]
    [SerializeField] private string promptText = "Press E to toggle";

    private bool playerInRange;

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

    private void Start()
    {
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        InteractionPrompt.Instance?.Show(promptText);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        InteractionPrompt.Instance?.Hide();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;
        if (GameManager.Instance == null) return;
        if (string.IsNullOrEmpty(targetObjectId))
        {
            Debug.LogError($"[HackLever] targetObjectId не задан на {name}!", this);
            return;
        }

        GameManager.Instance.World.Toggle(targetObjectId, targetPropertyType);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        if (GameManager.Instance == null) return;

        bool currentValue = GameManager.Instance.World.Get(targetObjectId, targetPropertyType);
        spriteRenderer.sprite = currentValue ? spriteOn : spriteOff;
    }
}
