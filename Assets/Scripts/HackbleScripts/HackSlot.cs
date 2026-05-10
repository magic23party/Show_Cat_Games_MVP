using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Слот для команды.
///
/// При старте сцены восстанавливает состояние из WorldState:
/// - Если в WorldState есть значение и в сцене есть команда с этим Value — помещает её в слот.
/// - Иначе — использует initialCommand (если задан).
/// - Иначе — слот пустой.
///
/// Поведение при изъятии:
/// - RememberLast: WorldState не трогается (для дверей).
/// - SetEmpty: WorldState получает emptyValue (для цветов).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackSlot : MonoBehaviour
{
    public enum EmptyMode { RememberLast, SetEmpty }

    [Header("Target")]
    [SerializeField] private string targetObjectId;
    [SerializeField] private string targetPropertyType = "Active";

    [Header("Empty Behavior")]
    [SerializeField] private EmptyMode onEmptyMode = EmptyMode.RememberLast;
    [SerializeField] private string emptyValue = "empty";

    [Header("Slot Position")]
    [SerializeField] private Transform commandAnchor;

    [Header("Initial State (используется если в WorldState ничего нет)")]
    [SerializeField] private HackCommand initialCommand;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("UI Hints")]
    [SerializeField] private string placePromptText = "Press E to place";
    [SerializeField] private string takePromptText = "Press E to take";

    private bool playerInRange;
    private PlayerCommandCarrier carrier;
    private HackCommand currentCommand;

    public Vector3 SlotPosition =>
        commandAnchor != null ? commandAnchor.position : transform.position;

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
        // Сначала пробуем восстановить состояние из WorldState
        bool restored = TryRestoreFromWorldState();

        if (restored) return;

        // Если не восстановили — используем initialCommand
        if (initialCommand != null)
        {
            currentCommand = initialCommand;
            initialCommand.PlaceInSlot(SlotPosition);
            ApplyValueToWorld(initialCommand.Value);
        }
        else
        {
            // Слот стартово пустой
            if (onEmptyMode == EmptyMode.SetEmpty)
                ApplyValueToWorld(emptyValue);
        }
    }

    /// <summary>
    /// Если в WorldState уже записано значение для targetObjectId — ищем в сцене команду
    /// с этим Value и помещаем её в слот. Возвращает true если удалось восстановить.
    /// </summary>
    private bool TryRestoreFromWorldState()
    {
        if (GameManager.Instance == null) return false;
        if (string.IsNullOrEmpty(targetObjectId)) return false;

        var ws = GameManager.Instance.World;
        // Используем флаг — было ли значение задано вообще.
        // GetString вернёт defaultValue если не задано, но defaultValue здесь зависит от вызова.
        string saved = ws.GetString(targetObjectId, targetPropertyType, null);
        if (string.IsNullOrEmpty(saved)) return false;

        // Если saved == emptyValue — слот должен быть пустым (для SetEmpty-режима)
        if (onEmptyMode == EmptyMode.SetEmpty && saved == emptyValue)
        {
            // Просто оставляем слот пустым, WorldState уже содержит emptyValue
            return true;
        }

        // Ищем в сцене команду с таким Value
        HackCommand found = FindFreeCommandWithValue(saved);
        if (found == null)
        {
            // Команды с таким значением нет — оставляем слот пустым,
            // но WorldState уже содержит saved (не трогаем)
            return true;
        }

        // Помещаем найденную команду в слот
        currentCommand = found;
        found.PlaceInSlot(SlotPosition);
        // WorldState уже содержит правильное значение, ApplyValueToWorld не нужен
        return true;
    }

    /// <summary>Ищет в текущей сцене HackCommand с указанным Value, который ещё не в слоте.</summary>
    private HackCommand FindFreeCommandWithValue(string value)
    {
        HackCommand[] all = FindObjectsByType<HackCommand>(FindObjectsSortMode.None);
        foreach (var cmd in all)
        {
            if (cmd == null) continue;
            if (cmd.IsInSlot) continue; // уже в каком-то другом слоте
            if (string.Equals(cmd.Value, value, System.StringComparison.OrdinalIgnoreCase))
                return cmd;
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var c = other.GetComponentInParent<PlayerCommandCarrier>();
        if (c == null) return;

        carrier = c;
        playerInRange = true;
        UpdatePrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        InteractionPrompt.Instance?.Hide();
    }

    private void Update()
    {
        if (playerInRange) UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        if (carrier == null) { InteractionPrompt.Instance?.Hide(); return; }

        if (carrier.HeldCommand != null)
            InteractionPrompt.Instance?.Show(placePromptText);
        else if (currentCommand != null)
            InteractionPrompt.Instance?.Show(takePromptText);
        else
            InteractionPrompt.Instance?.Hide();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || carrier == null) return;

        if (carrier.HeldCommand != null)
        {
            HackCommand incoming = carrier.HeldCommand;

            if (currentCommand != null && currentCommand != incoming)
                currentCommand.EjectFromSlot(carrier.DropPosition);

            incoming.PlaceInSlot(SlotPosition);
            currentCommand = incoming;

            ApplyValueToWorld(currentCommand.Value);
            UpdatePrompt();
            return;
        }

        if (currentCommand != null)
        {
            currentCommand.TakeFromSlot(carrier);
            currentCommand = null;

            if (onEmptyMode == EmptyMode.SetEmpty)
                ApplyValueToWorld(emptyValue);

            UpdatePrompt();
        }
    }

    private void ApplyValueToWorld(string value)
    {
        if (GameManager.Instance == null) return;
        if (string.IsNullOrEmpty(targetObjectId))
        {
            Debug.LogError($"[HackSlot] targetObjectId не задан на {name}!", this);
            return;
        }
        GameManager.Instance.World.SetString(targetObjectId, targetPropertyType, value);
    }
}
