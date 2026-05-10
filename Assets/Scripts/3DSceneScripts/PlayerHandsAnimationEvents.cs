using UnityEngine;

/// <summary>
/// Мост между Animation Event и GameManager.
/// Вешается рядом с Animator на руках игрока.
///
/// КАК ИСПОЛЬЗОВАТЬ:
/// 1. Открой клип анимации рук в окне Animation.
/// 2. На последнем кадре добавь Animation Event.
/// 3. В поле Function выбери NotifyAnimationFinished.
/// 4. Готово — GameManager получит сигнал в момент окончания анимации.
/// </summary>
public class PlayerHandsAnimationEvents : MonoBehaviour
{
    /// <summary>
    /// Вызывается из Animation Event в конце клипа "EnterBug".
    /// </summary>
    public void NotifyAnimationFinished()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyAnimationFinished();
        else
            Debug.LogWarning("[PlayerHandsAnimationEvents] GameManager.Instance == null");
    }
}
