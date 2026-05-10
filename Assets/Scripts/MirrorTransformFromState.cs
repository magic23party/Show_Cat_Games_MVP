using UnityEngine;

/// <summary>
/// Зеркалит scale и rotation указанного 3D-объекта (через WorldState) на свой transform.
/// Используется для отображения в 2D-сцене текущего состояния куба-головоломки.
///
/// Ключи в WorldState:
/// - "{sourceObjectId}:Scale.X", "Scale.Y", "Scale.Z"
/// - "{sourceObjectId}:Rotation.X", "Rotation.Y", "Rotation.Z"
///
/// Для 2D-спрайта Scale.Z не имеет смысла (галка mirrorScaleZ выкл по умолчанию).
/// Rotation работает по всем осям (по X/Y будет "сжатие в перспективе" — выглядит интересно).
/// </summary>
public class MirrorTransformFromState : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("ID объекта в WorldState (тот же, что в HackableObject в 3D).")]
    [SerializeField] private string sourceObjectId;

    [Header("Defaults (стартовые значения, если в WorldState ничего нет)")]
    [SerializeField] private int defaultScale = 1;
    [SerializeField] private int defaultRotation = 0;

    [Header("Multipliers (как в HackableTransform — должны совпадать!)")]
    [Tooltip("Множитель Scale. Должен совпадать с Scale Multiplier на HackableTransform 3D-куба.")]
    [SerializeField] private float scaleMultiplier = 1f;

    [Tooltip("Множитель Rotation. Должен совпадать с Rotation Multiplier на HackableTransform 3D-куба.")]
    [SerializeField] private float rotationMultiplier = 1f;

    [Header("Mirror Settings")]
    [Tooltip("Зеркалить Scale.X?")]
    [SerializeField] private bool mirrorScaleX = true;

    [Tooltip("Зеркалить Scale.Y?")]
    [SerializeField] private bool mirrorScaleY = true;

    [Tooltip("Зеркалить Scale.Z? Для 2D-спрайта обычно не нужно.")]
    [SerializeField] private bool mirrorScaleZ = false;

    [Tooltip("Зеркалить Rotation.X?")]
    [SerializeField] private bool mirrorRotationX = true;

    [Tooltip("Зеркалить Rotation.Y?")]
    [SerializeField] private bool mirrorRotationY = true;

    [Tooltip("Зеркалить Rotation.Z?")]
    [SerializeField] private bool mirrorRotationZ = true;

    private Vector3 baseScale;
    private Vector3 baseRotation;

    private void Awake()
    {
        // Запоминаем стартовые значения transform для тех осей, которые НЕ зеркалим
        baseScale = transform.localScale;
        baseRotation = transform.localEulerAngles;
    }

    private void Start()
    {
        if (GameManager.Instance == null) return;

        ApplyAll();

        // Подписка на изменения
        GameManager.Instance.World.OnIntChanged += HandleIntChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.World.OnIntChanged -= HandleIntChanged;
    }

    private void HandleIntChanged(string objectId, string propertyType, int newValue)
    {
        if (objectId != sourceObjectId) return;

        if (propertyType.StartsWith("Scale.") || propertyType.StartsWith("Rotation."))
            ApplyAll();
    }

    private void ApplyAll()
    {
        var ws = GameManager.Instance.World;

        // Стартуем с baseScale/baseRotation, и переопределяем только зеркалимые оси
        Vector3 scale = baseScale;
        Vector3 rotation = baseRotation;

        if (mirrorScaleX)
            scale.x = ws.GetInt(sourceObjectId, "Scale.X", defaultScale) * scaleMultiplier;

        if (mirrorScaleY)
            scale.y = ws.GetInt(sourceObjectId, "Scale.Y", defaultScale) * scaleMultiplier;

        if (mirrorScaleZ)
            scale.z = ws.GetInt(sourceObjectId, "Scale.Z", defaultScale) * scaleMultiplier;

        if (mirrorRotationX)
            rotation.x = ws.GetInt(sourceObjectId, "Rotation.X", defaultRotation) * rotationMultiplier;

        if (mirrorRotationY)
            rotation.y = ws.GetInt(sourceObjectId, "Rotation.Y", defaultRotation) * rotationMultiplier;

        if (mirrorRotationZ)
            rotation.z = ws.GetInt(sourceObjectId, "Rotation.Z", defaultRotation) * rotationMultiplier;

        transform.localScale = scale;
        transform.localEulerAngles = rotation;
    }
}
