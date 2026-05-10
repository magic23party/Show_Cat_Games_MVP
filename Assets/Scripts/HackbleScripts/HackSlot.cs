using UnityEngine;

/// <summary>
/// Слот для команды. Пассивный — не слушает E.
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

    [Header("Initial State")]
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

        if (onEmptyMode == EmptyMode.SetEmpty && saved == emptyValue)
            return;

        HackCommand found = FindFreeCommandWithValue(saved);
        if (found != null)
            PlaceCommandInternal(found);
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

    public void PlaceCommand(HackCommand incoming)
    {
        if (incoming == null) return;

        Vector3 oldDropPos = SlotPosition;
        var carrier = FindAnyObjectByType<PlayerCommandCarrier>();
        if (carrier != null) oldDropPos = carrier.DropPosition;

        if (CurrentCommand != null && CurrentCommand != incoming)
            CurrentCommand.EjectFromSlot(oldDropPos);

        if (carrier != null) carrier.ClearHeld();

        PlaceCommandInternal(incoming);
        ApplyValueToWorld(incoming.Value);

        // SFX: команду подключили к слоту
        SoundManager.Instance?.PlaySFX("CmdOn");
    }

    private void PlaceCommandInternal(HackCommand cmd)
    {
        CurrentCommand = cmd;
        cmd.PlaceInSlot(SlotPosition);
    }

    public HackCommand TakeCommand()
    {
        if (CurrentCommand == null) return null;

        HackCommand taken = CurrentCommand;
        CurrentCommand = null;

        if (onEmptyMode == EmptyMode.SetEmpty)
            ApplyValueToWorld(emptyValue);

        // SFX: команду отключили от слота
        SoundManager.Instance?.PlaySFX("CmdOff");

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
