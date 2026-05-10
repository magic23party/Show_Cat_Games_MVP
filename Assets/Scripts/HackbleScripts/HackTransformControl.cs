using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контроль одной оси Scale или Rotation в 2D-сцене.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackTransformControl : MonoBehaviour
{
    public enum AxisType { Scale, Rotation }
    public enum Axis { X, Y, Z }

    [Header("Target")]
    public string targetObjectId;
    public AxisType axisType = AxisType.Scale;
    public Axis axis = Axis.X;

    [Header("Range")]
    public int minValue = 1;
    public int maxValue = 3;
    public int step = 1;

    [Header("UI")]
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
        propertyType = $"{axisType}.{axis}";
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
        if (next > maxValue) next = minValue;

        ws.SetInt(targetObjectId, propertyType, next);
        UpdateLabel();

        // SFX: переключение значения
        SoundManager.Instance?.PlaySFX("Switch");
    }

    private void UpdateLabel()
    {
        if (tmpLabel == null) return;
        if (GameManager.Instance == null) return;

        int value = GameManager.Instance.World.GetInt(targetObjectId, propertyType, minValue);
        tmpLabel.text = value.ToString();
    }
}
