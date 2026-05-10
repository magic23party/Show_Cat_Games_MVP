using UnityEngine;

/// <summary>
/// Слот для команды. Пассивный — не слушает E.
/// 
/// Логика взаимодействия в PlayerCommandCarrier на игроке.
/// Этот скрипт:
/// - сообщает carrier когда игрок зашёл/вышел из зоны (через триггер)
/// - предоставляет методы PlaceCommand / TakeCommand
/// - применяет значение текущей команды к WorldState
/// - при старте восстанавливает состояние из WorldState (или использует initialCommand)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackSlot : MonoBehaviour
{
    public enum EmptyMode
    {
        /// <summary>Пустой слот → WorldState не трогается (для дверей).</summary>
        RememberLast,
        /// <summary>Пустой слот → WorldState получает emptyValue (для цветов).</summary>
        SetEmpty
    }

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

    public HackCommand CurrentCommand { get; private set; }

    public Vector3 SlotPosition =>
        commandAnchor != null ? commandAnchor.position : transform.position;

    private void Start()
    {
        InitializeFromWorldState();
    }

    private void InitializeFromWorldState()
    {
        if (GameManager.Instance == null) return;
        var ws = GameManager.Instance.World;

        string saved = ws.GetString(targetObjectId, targetPropertyType, null);

        // КЕЙС 1: WorldState пустой → используем initialCommand
        if (string.IsNullOrEmpty(saved))
        {
            if (initialCommand != null)
            {
                PlaceCommandInternal(initialCommand);
                ApplyValueToWorld(initialCommand.Value);
            }
            else
            {
                if (onEmptyMode == EmptyMode.SetEmpty)
                    ApplyValueToWorld(emptyValue);
            }
            return;
        }

        // КЕЙС 2: В WorldState есть значение
        if (onEmptyMode == EmptyMode.SetEmpty && saved == emptyValue)
            return; // слот пустой

        HackCommand found = FindFreeCommandWithValue(saved);
        if (found != null)
            PlaceCommandInternal(found);
        // Если команды с таким значением нет — слот пустой, WorldState не меняем
    }

    private HackCommand FindFreeCommandWithValue(string value)
    {
        HackCommand[] all = FindObjectsByType<HackCommand>(FindObjectsSortMode.None);
        foreach (var cmd in all)
        {
            if (cmd == null) continue;
            if (cmd.IsInSlot) continue;
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
        c.NotifySlotEnter(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var c = other.GetComponentInParent<PlayerCommandCarrier>();
        if (c == null) return;
        c.NotifySlotExit(this);
    }

    /// <summary>
    /// Поместить команду в слот. Вызывается из PlayerCommandCarrier.
    /// Если слот занят — старая команда выпадает в позицию игрока.
    /// </summary>
    public void PlaceCommand(HackCommand incoming)
    {
        if (incoming == null) return;

        // Определяем куда выкинуть старую (если она есть)
        Vector3 oldDropPos = SlotPosition;
        var carrier = FindAnyObjectByType<PlayerCommandCarrier>();
        if (carrier != null) oldDropPos = carrier.DropPosition;

        if (CurrentCommand != null && CurrentCommand != incoming)
            CurrentCommand.EjectFromSlot(oldDropPos);

        // carrier теперь не несёт incoming
        if (carrier != null) carrier.ClearHeld();

        PlaceCommandInternal(incoming);
        ApplyValueToWorld(incoming.Value);
    }

    private void PlaceCommandInternal(HackCommand cmd)
    {
        CurrentCommand = cmd;
        cmd.PlaceInSlot(SlotPosition);
    }

    /// <summary>
    /// Забрать команду из слота. Возвращает команду или null если слот пустой.
    /// Вызывается из PlayerCommandCarrier.
    /// </summary>
    public HackCommand TakeCommand()
    {
        if (CurrentCommand == null) return null;

        HackCommand taken = CurrentCommand;
        CurrentCommand = null;

        if (onEmptyMode == EmptyMode.SetEmpty)
            ApplyValueToWorld(emptyValue);

        return taken;
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
