using UnityEngine;

/// <summary>
/// Применяет к transform куба значения Scale и Rotation, хранящиеся в WorldState.
///
/// Использует ключи:
/// - "{objectId}:Scale.X", "Scale.Y", "Scale.Z"
/// - "{objectId}:Rotation.X", "Rotation.Y", "Rotation.Z"
///
/// Эти же ключи используют HackTransformControl-объекты в 2D-сцене.
///
/// При старте применяет сохранённые значения. Подписывается на изменения и
/// обновляет transform в реальном времени.
/// </summary>
[RequireComponent(typeof(HackableObject))]
public class HackableTransform : MonoBehaviour
{
    [Header("Defaults (стартовые значения, если в WorldState ничего нет)")]
    [Tooltip("Стартовое значение Scale по умолчанию (по всем осям).")]
    public int defaultScale = 1;

    [Tooltip("Стартовое значение Rotation в градусах (по всем осям).")]
    public int defaultRotation = 0;

    [Header("Multipliers (как int-значение из 2D превращается в реальный scale/rotation)")]
    [Tooltip("Каждая единица 'Scale' в 2D = это значение в реальном scale 3D-куба. " +
             "Например, 1 → scale 1, 2 → scale 2 (если scaleMultiplier=1). " +
             "Если хочешь чтобы 1=0.5, 2=1.0, 3=1.5 — поставь 0.5.")]
    public float scaleMultiplier = 1f;

    [Tooltip("Каждая единица 'Rotation' в 2D = это значение в градусах. " +
             "Если игрок в 2D имеет шаг 15° — оставь 1 (тогда WorldState хранит сами градусы). " +
             "Если игрок имеет шаг 1 (значения 0,1,2,3...) — поставь 15.")]
    public float rotationMultiplier = 1f;

    private HackableObject owner;

    private void Awake()
    {
        owner = GetComponent<HackableObject>();
    }

    private void Start()
    {
        if (GameManager.Instance == null) return;

        // Применяем текущие значения
        ApplyAll();

        // Подписываемся на изменения
        GameManager.Instance.World.OnIntChanged += HandleIntChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.World.OnIntChanged -= HandleIntChanged;
    }

    private void HandleIntChanged(string objectId, string propertyType, int newValue)
    {
        if (objectId != owner.objectId) return;

        // Любое изменение Scale.* или Rotation.* — пересчитываем всё
        if (propertyType.StartsWith("Scale.") || propertyType.StartsWith("Rotation."))
            ApplyAll();
    }

    private void ApplyAll()
    {
        var ws = GameManager.Instance.World;

        int sx = ws.GetInt(owner.objectId, "Scale.X", defaultScale);
        int sy = ws.GetInt(owner.objectId, "Scale.Y", defaultScale);
        int sz = ws.GetInt(owner.objectId, "Scale.Z", defaultScale);

        int rx = ws.GetInt(owner.objectId, "Rotation.X", defaultRotation);
        int ry = ws.GetInt(owner.objectId, "Rotation.Y", defaultRotation);
        int rz = ws.GetInt(owner.objectId, "Rotation.Z", defaultRotation);

        transform.localScale = new Vector3(
            sx * scaleMultiplier,
            sy * scaleMultiplier,
            sz * scaleMultiplier
        );

        transform.localEulerAngles = new Vector3(
            rx * rotationMultiplier,
            ry * rotationMultiplier,
            rz * rotationMultiplier
        );
    }
}
