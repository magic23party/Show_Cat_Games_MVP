using System;
using UnityEngine;

/// <summary>
/// Следит за состоянием нескольких объектов в WorldState.
/// Когда все условия выполнены (значения совпадают с expected) — выполняет действие.
/// Когда условие нарушено — действие отменяется (если revertible = true).
///
/// Используется для головоломки: 3 куба должны быть определённых цветов → дверь открывается.
/// </summary>
public class DoorPuzzleChecker : MonoBehaviour
{
    [Serializable]
    public class Condition
    {
        [Tooltip("Object ID объекта в WorldState (например, 'cube_1').")]
        public string objectId;

        [Tooltip("Property type, например 'Color'.")]
        public string propertyType = "Color";

        [Tooltip("Ожидаемое значение, например 'purple'.")]
        public string expectedValue;
    }

    [Header("Conditions (все должны выполняться одновременно)")]
    [SerializeField] private Condition[] conditions;

    [Header("Action — что делаем когда паззл решён / разрешён")]
    [Tooltip("GameObject двери (или другого объекта), который включается/выключается. Например — preграда, которая исчезает.")]
    [SerializeField] private GameObject doorBlocker;

    [Tooltip("true: doorBlocker выключается когда паззл решён. false: включается.")]
    [SerializeField] private bool deactivateWhenSolved = true;

    [Header("Behavior")]
    [Tooltip("Если true — действие отменяется когда паззл сломан. " +
             "Если false — один раз решил, дверь остаётся открытой навсегда.")]
    [SerializeField] private bool revertible = true;

    private bool isSolved = false;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.World.OnStringChanged += HandleChanged;
        GameManager.Instance.World.OnBoolChanged += HandleBoolChanged;
        GameManager.Instance.World.OnIntChanged += HandleIntChanged;

        CheckAndApply();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.World.OnStringChanged -= HandleChanged;
            GameManager.Instance.World.OnBoolChanged -= HandleBoolChanged;
            GameManager.Instance.World.OnIntChanged -= HandleIntChanged;
        }
    }

    private void HandleChanged(string objectId, string propertyType, string newValue) => CheckAndApply();
    private void HandleBoolChanged(string objectId, string propertyType, bool newValue) => CheckAndApply();
    private void HandleIntChanged(string objectId, string propertyType, int newValue) => CheckAndApply();

    private void CheckAndApply()
    {
        if (conditions == null || conditions.Length == 0) return;
        if (GameManager.Instance == null) return;

        var ws = GameManager.Instance.World;
        bool allMatch = true;

        foreach (var cond in conditions)
        {
            if (string.IsNullOrEmpty(cond.objectId)) { allMatch = false; break; }

            string current = ws.GetString(cond.objectId, cond.propertyType, "");
            if (!string.Equals(current, cond.expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                allMatch = false;
                break;
            }
        }

        if (allMatch && !isSolved)
        {
            isSolved = true;
            ApplyAction(true);
        }
        else if (!allMatch && isSolved && revertible)
        {
            isSolved = false;
            ApplyAction(false);
        }
    }

    private void ApplyAction(bool solved)
    {
        if (doorBlocker == null) return;

        // Если deactivateWhenSolved=true: при solved → false (deactivate), при не solved → true (activate)
        bool shouldBeActive = deactivateWhenSolved ? !solved : solved;
        doorBlocker.SetActive(shouldBeActive);
    }
}
