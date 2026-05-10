using UnityEngine;

/// <summary>
/// Команда (например "True", "False", "red"). Пассивный объект.
/// 
/// Логика хватания/отпускания обрабатывается в PlayerCommandCarrier (на игроке).
/// Этот скрипт только:
/// - сообщает carrier о появлении/уходе игрока (через триггер)
/// - выполняет команды carrier-а (AttachToCarrier, DropAt, PlaceInSlot и т.д.)
/// - визуально следует за carrier-ом пока несётся
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackCommand : MonoBehaviour
{
    [Header("Command Value")]
    [SerializeField] private string commandValue = "true";

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    public string Value => commandValue;
    public bool IsInSlot { get; private set; }
    public bool IsHeld => carrier != null && carrier.HeldCommand == this;

    private PlayerCommandCarrier carrier;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var c = other.GetComponentInParent<PlayerCommandCarrier>();
        if (c == null) return;
        c.NotifyCommandEnter(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var c = other.GetComponentInParent<PlayerCommandCarrier>();
        if (c == null) return;
        c.NotifyCommandExit(this);
    }

    /// <summary>Вызывается из PlayerCommandCarrier когда игрок берёт команду.</summary>
    public void AttachToCarrier(PlayerCommandCarrier c)
    {
        carrier = c;
        IsInSlot = false;
    }

    /// <summary>Уронить в указанную позицию. Команда больше не несётся и не в слоте.</summary>
    public void DropAt(Vector3 worldPos)
    {
        transform.position = worldPos;
        IsInSlot = false;
        // carrier не нулим — он остаётся как ссылка на последнего носителя, но HeldCommand уже null
    }

    /// <summary>Поставить в слот.</summary>
    public void PlaceInSlot(Vector3 worldPos)
    {
        transform.position = worldPos;
        IsInSlot = true;
    }

    /// <summary>Команду выкинули из слота (заменили другой).</summary>
    public void EjectFromSlot(Vector3 worldPos)
    {
        transform.position = worldPos;
        IsInSlot = false;
    }

    private void LateUpdate()
    {
        // Если несёмся — следуем за carrier
        if (IsHeld && carrier != null)
            transform.position = carrier.CarryPosition;
    }
}
