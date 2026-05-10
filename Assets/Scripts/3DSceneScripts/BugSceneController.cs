using UnityEngine;

/// <summary>
/// Вешается на корневой объект каждой 2D-сцены бага.
/// Когда уровень пройден — вызови ReportLevelComplete() из любого скрипта победы
/// (например, из триггера финиша или после убийства всех врагов).
/// </summary>
public class BugSceneController : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Если включено — нажатие на эту клавишу мгновенно завершает уровень. Удобно для тестов.")]
    [SerializeField] private bool enableDebugWinKey = false;
    [SerializeField] private KeyCode debugWinKey = KeyCode.F10;

    private bool isCompleted = false;

    private void Update()
    {
        if (enableDebugWinKey && Input.GetKeyDown(debugWinKey))
            ReportLevelComplete();
    }

    /// <summary>
    /// Вызови этот метод когда игрок прошёл 2D-уровень.
    /// Можно дёрнуть из любого места: коллайдер-финиш, скрипт врага, кнопка и т.д.
    /// </summary>
    public void ReportLevelComplete()
    {
        Debug.Log($"[BugSceneController] ReportLevelComplete вызван. isCompleted={isCompleted}");
        if (isCompleted) return;
        isCompleted = true;

        if (GameManager.Instance != null)
            GameManager.Instance.ExitBug();
        else
            Debug.LogError("[BugSceneController] GameManager.Instance == null!");
    }
}
