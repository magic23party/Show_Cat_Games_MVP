using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроль одной оси Scale или Rotation в 2D-сцене.
/// Игрок подходит к цифре, жмёт E — значение +step (циклически возвращается к min при превышении max).
///
/// Настройка:
/// - На GameObject должен быть Collider2D с Is Trigger = true.
/// - В инспекторе указываешь: какой объект, какой тип (Scale/Rotation), какая ось (X/Y/Z), min/max/step.
/// - tmpLabel — TMP-текст, который покажет текущее значение.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackTransformControl : MonoBehaviour
{
    public enum AxisType { Scale, Rotation }
    public enum Axis { X, Y, Z }

    [Header("Target")]
    [Tooltip("ID объекта в 3D (тот же, что в HackableObject.objectId).")]
    public string targetObjectId;

    [Tooltip("Что меняем: Scale или Rotation.")]
    public AxisType axisType = AxisType.Scale;

    [Tooltip("Какую ось меняем: X, Y или Z.")]
    public Axis axis = Axis.X;

    [Header("Range (настраивай как нужно)")]
    [Tooltip("Минимальное значение (стартовое). Для Scale обычно 1, для Rotation обычно 0.")]
    public int minValue = 1;

    [Tooltip("Максимальное значение. После него возвращается к minValue.")]
    public int maxValue = 3;

    [Tooltip("Шаг изменения. Для Scale обычно 1, для Rotation в твоём случае 15 (градусов).")]
    public int step = 1;

    [Header("UI")]
    [Tooltip("TMP-текст, который покажет текущее значение. Если null — скрипт сам найдёт TMP в детях.")]
    public TMP_Text tmpLabel;

    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("Input")]
    public InputActionReference interactAction;

    [Header("UI Hint")]
    public string promptText = "Press E to change";

    private bool playerInRange;
    private string propertyType;

    private void Awake()
    {
        // Формируем propertyType из axisType + axis (например "Scale.X")
        propertyType = $"{axisType}.{axis}";

        // Если TMP не назначен — пробуем найти в детях
        if (tmpLabel == null)
            tmpLabel = GetComponentInChildren<TMP_Text>();
    }

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
        UpdateLabel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        InteractionPrompt.Instance?.Show(promptText);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        InteractionPrompt.Instance?.Hide();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange) return;
        if (GameManager.Instance == null) return;
        if (string.IsNullOrEmpty(targetObjectId))
        {
            Debug.LogError($"[HackTransformControl] targetObjectId не задан на {name}!", this);
            return;
        }

        var ws = GameManager.Instance.World;
        int current = ws.GetInt(targetObjectId, propertyType, minValue);
        int next = current + step;

        // Циклический сброс при превышении максимума
        if (next > maxValue)
            next = minValue;

        ws.SetInt(targetObjectId, propertyType, next);
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (tmpLabel == null) return;
        if (GameManager.Instance == null) return;

        int value = GameManager.Instance.World.GetInt(targetObjectId, propertyType, minValue);
        tmpLabel.text = value.ToString();
    }
}
