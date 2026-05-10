using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Перетаскиваемая команда (например "True", "False").
/// 
/// Без физики — команда просто меняет позицию через transform.
/// Когда несётся — следует за carrier.CarryPosition (над головой).
/// Когда отпускается вне слота — встаёт в carrier.DropPosition (центр игрока).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackCommand : MonoBehaviour
{
    [Header("Command Value")]
    [SerializeField] private string commandValue = "true";

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("UI Hint")]
    [SerializeField] private string grabPromptText = "Press E to grab";

    public string Value => commandValue;
    public bool IsInSlot { get; private set; }
    public bool IsHeld => carrier != null && carrier.HeldCommand == this;

    private bool playerInRange;
    private PlayerCommandCarrier carrier;
    private bool justTakenFromSlot;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (IsHeld || IsInSlot) return;

        var c = other.GetComponentInParent<PlayerCommandCarrier>();
        if (c == null) return;

        carrier = c;
        playerInRange = true;

        if (carrier.HeldCommand == null)
            InteractionPrompt.Instance?.Show(grabPromptText);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        InteractionPrompt.Instance?.Hide();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (carrier == null) return;
        if (justTakenFromSlot) return;
        if (IsInSlot) return;

        if (IsHeld)
        {
            DropAtPlayer();
            return;
        }

        if (playerInRange && carrier.HeldCommand == null)
            PickUp();
    }

    public void PickUp()
    {
        if (carrier == null) return;
        carrier.SetHeld(this);
        IsInSlot = false;
        InteractionPrompt.Instance?.Hide();
    }

    /// <summary>Уронить команду в позицию игрока (центр).</summary>
    public void DropAtPlayer()
    {
        if (carrier == null) return;
        transform.position = carrier.DropPosition;
        carrier.ClearHeld();
        IsInSlot = false;
    }

    public void PlaceInSlot(Vector3 worldPos)
    {
        if (carrier != null && carrier.HeldCommand == this)
            carrier.ClearHeld();

        transform.position = worldPos;
        IsInSlot = true;
    }

    public void TakeFromSlot(PlayerCommandCarrier byCarrier)
    {
        carrier = byCarrier;  // <-- устанавливаем carrier явно
        IsInSlot = false;
        PickUp();
        justTakenFromSlot = true;
    }

    public void EjectFromSlot(Vector3 worldPos)
    {
        transform.position = worldPos;
        IsInSlot = false;
    }

    private void LateUpdate()
    {
        if (IsHeld && carrier != null)
            transform.position = carrier.CarryPosition;

        justTakenFromSlot = false;
    }
}
