using UnityEngine;

/// <summary>
/// Вешается на финишный триггер в 2D-сцене.
/// Требует Collider2D с Is Trigger = true.
/// Когда игрок входит в зону — завершает уровень.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelFinish : MonoBehaviour
{
    [Tooltip("Ссылка на BugSceneController в этой же 2D-сцене.")]
    [SerializeField] private BugSceneController sceneController;

    [Tooltip("Тег 2D-игрока. Должен совпадать с тегом на твоём 2D-персонаже.")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[LevelFinish] OnTriggerEnter2D с {other.name}, tag={other.tag}");
        if (!other.CompareTag(playerTag)) return;

        if (sceneController != null)
        {
            Debug.Log("[LevelFinish] Вызываю ReportLevelComplete");
            sceneController.ReportLevelComplete();
        }
        else
            Debug.LogError("[LevelFinish] BugSceneController не назначен!", this);
    }
}
