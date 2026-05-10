using UnityEngine;

/// <summary>
/// Вешается на 2D-игрока. Хранит ссылку на текущую несомую команду.
/// </summary>
public class PlayerCommandCarrier : MonoBehaviour
{
    [Header("Carry Position (где висит команда когда несётся)")]
    [Tooltip("Куда визуально 'прикрепляется' несомая команда (например, над головой).")]
    [SerializeField] private Transform carryAnchor;

    [Tooltip("Смещение от игрока, если carryAnchor не задан.")]
    [SerializeField] private Vector3 carryOffset = new Vector3(0, 1f, 0);

    [Header("Drop Position (куда падает команда когда её роняют)")]
    [Tooltip("Точка, куда команда становится при отпускании. По умолчанию — позиция игрока.")]
    [SerializeField] private Transform dropAnchor;

    [Tooltip("Смещение от игрока, если dropAnchor не задан. По умолчанию (0,0,0) — центр игрока.")]
    [SerializeField] private Vector3 dropOffset = Vector3.zero;

    public HackCommand HeldCommand { get; private set; }

    /// <summary>Где висит команда пока несётся.</summary>
    public Vector3 CarryPosition =>
        carryAnchor != null ? carryAnchor.position : transform.position + carryOffset;

    /// <summary>Куда команда становится при отпускании (центр игрока).</summary>
    public Vector3 DropPosition =>
        dropAnchor != null ? dropAnchor.position : transform.position + dropOffset;

    public void SetHeld(HackCommand command) => HeldCommand = command;
    public void ClearHeld() => HeldCommand = null;
}
