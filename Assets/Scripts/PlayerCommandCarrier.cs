using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Единая логика взаимодействия игрока с командами и слотами.
///
/// Слушает E. Когда нажимается:
/// - Если несёт команду + рядом слот → ставит команду в слот.
/// - Если несёт команду + НЕ рядом со слотом → роняет команду в позицию игрока.
/// - Если не несёт + рядом слот с командой → берёт команду из слота.
/// - Если не несёт + рядом свободная команда → берёт её.
///
/// HackCommand и HackSlot регистрируют себя в этом carrier через триггеры
/// (NotifyEnter/NotifyExit), сами больше не слушают E.
/// </summary>
public class PlayerCommandCarrier : MonoBehaviour
{
    [Header("Carry / Drop Positions")]
    [Tooltip("Куда крепится команда когда несётся (над головой).")]
    [SerializeField] private Transform carryAnchor;
    [SerializeField] private Vector3 carryOffset = new Vector3(0, 1f, 0);

    [Tooltip("Куда падает команда когда роняется (центр игрока).")]
    [SerializeField] private Transform dropAnchor;
    [SerializeField] private Vector3 dropOffset = Vector3.zero;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    [Header("UI Hints")]
    [SerializeField] private string grabPromptText = "Press E to grab";
    [SerializeField] private string placePromptText = "Press E to place";
    [SerializeField] private string takePromptText = "Press E to take";

    public HackCommand HeldCommand { get; private set; }

    public Vector3 CarryPosition =>
        carryAnchor != null ? carryAnchor.position : transform.position + carryOffset;

    public Vector3 DropPosition =>
        dropAnchor != null ? dropAnchor.position : transform.position + dropOffset;

    // Списки объектов, в зону которых мы зашли
    private readonly HashSet<HackCommand> commandsInRange = new HashSet<HackCommand>();
    private readonly HashSet<HackSlot> slotsInRange = new HashSet<HackSlot>();

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

    public void NotifyCommandEnter(HackCommand cmd) => commandsInRange.Add(cmd);
    public void NotifyCommandExit(HackCommand cmd) => commandsInRange.Remove(cmd);
    public void NotifySlotEnter(HackSlot slot) => slotsInRange.Add(slot);
    public void NotifySlotExit(HackSlot slot) => slotsInRange.Remove(slot);

    private void Update()
    {
        UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        // Приоритет 1: несём команду + рядом слот → "place"
        if (HeldCommand != null && GetNearestSlot() != null)
        {
            InteractionPrompt.Instance?.Show(placePromptText);
            return;
        }

        // Приоритет 2: не несём + рядом слот с командой → "take"
        if (HeldCommand == null)
        {
            HackSlot s = GetNearestSlot();
            if (s != null && s.CurrentCommand != null)
            {
                InteractionPrompt.Instance?.Show(takePromptText);
                return;
            }

            // Приоритет 3: не несём + рядом свободная команда (не в слоте) → "grab"
            HackCommand c = GetNearestFreeCommand();
            if (c != null)
            {
                InteractionPrompt.Instance?.Show(grabPromptText);
                return;
            }
        }

        // Иначе скрываем
        InteractionPrompt.Instance?.Hide();
    }

    private HackSlot GetNearestSlot()
    {
        // Удаляем мёртвые ссылки и возвращаем первый
        slotsInRange.RemoveWhere(s => s == null);
        foreach (var s in slotsInRange) return s;
        return null;
    }

    private HackCommand GetNearestFreeCommand()
    {
        commandsInRange.RemoveWhere(c => c == null);
        foreach (var c in commandsInRange)
        {
            if (c == null) continue;
            if (c.IsInSlot) continue; // в слоте — не считается свободной
            if (c == HeldCommand) continue;
            return c;
        }
        return null;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        // Сценарий 1: несём команду
        if (HeldCommand != null)
        {
            HackSlot slot = GetNearestSlot();
            if (slot != null)
            {
                // Ставим в слот
                slot.PlaceCommand(HeldCommand);
                // (HeldCommand станет null внутри PlaceCommand)
            }
            else
            {
                // Роняем
                HeldCommand.DropAt(DropPosition);
                ClearHeld();
            }
            UpdatePrompt();
            return;
        }

        // Сценарий 2: не несём, рядом слот с командой → берём
        HackSlot nearbySlot = GetNearestSlot();
        if (nearbySlot != null && nearbySlot.CurrentCommand != null)
        {
            HackCommand cmd = nearbySlot.TakeCommand();
            if (cmd != null) PickUp(cmd);
            UpdatePrompt();
            return;
        }

        // Сценарий 3: не несём, рядом свободная команда → берём
        HackCommand free = GetNearestFreeCommand();
        if (free != null)
        {
            PickUp(free);
            UpdatePrompt();
        }
    }

    public void PickUp(HackCommand cmd)
    {
        HeldCommand = cmd;
        cmd.AttachToCarrier(this);
    }

    public void ClearHeld()
    {
        HeldCommand = null;
    }
}
