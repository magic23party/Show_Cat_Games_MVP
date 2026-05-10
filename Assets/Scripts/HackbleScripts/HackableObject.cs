using UnityEngine;

/// <summary>
/// Помечает 3D-объект как "хакабельный".
/// На том же GameObject должны висеть HackableProperty-компоненты
/// (HackableCollision, HackableGravity, HackableVisibility и т.д.).
///
/// При старте каждое свойство применит к себе значение из WorldState
/// (с учётом сохранённого состояния из предыдущих визитов в 2D).
/// </summary>
public class HackableObject : MonoBehaviour
{
    [Tooltip("Уникальный ID объекта. Например: door_main, box_red, npc_guard. Должен быть уникален в проекте.")]
    public string objectId;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(objectId))
            Debug.LogWarning($"[HackableObject] objectId не задан на {name}!", this);
    }
}
