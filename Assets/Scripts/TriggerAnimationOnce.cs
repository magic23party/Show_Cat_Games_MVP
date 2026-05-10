using UnityEngine;

/// <summary>
/// При касании указанного объекта вызывает SetTrigger на Animator.
/// Срабатывает только ОДИН раз — после этого скрипт отключается.
///
/// Требует Collider с Is Trigger = true на этом объекте.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerAnimationOnce : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Объект, чей коллайдер должен войти в триггер. Только этот объект сработает.")]
    [SerializeField] private GameObject targetObject;

    [Tooltip("Animator, на котором будет вызван SetTrigger.")]
    [SerializeField] private Animator targetAnimator;

    [Header("Animator")]
    [Tooltip("Имя триггера в Animator. Должен соответствовать параметру Trigger в Animator Controller.")]
    [SerializeField] private string triggerName = "keyActive";

    private bool fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (fired) return;
        if (targetObject == null || targetAnimator == null) return;

        // Проверяем, что коллайдер принадлежит указанному объекту
        // (с учётом дочерних коллайдеров — берём корень)
        if (other.gameObject != targetObject &&
            other.transform.root.gameObject != targetObject &&
            other.GetComponentInParent<Transform>()?.gameObject != targetObject)
        {
            // Альтернативная проверка: если targetObject где-то в иерархии other
            if (!IsChildOrSelf(other.transform, targetObject.transform))
                return;
        }

        fired = true;
        targetAnimator.SetTrigger(triggerName);
    }

    /// <summary>Проверка: является ли check либо самим target, либо его потомком.</summary>
    private bool IsChildOrSelf(Transform check, Transform target)
    {
        Transform t = check;
        while (t != null)
        {
            if (t == target) return true;
            t = t.parent;
        }
        return false;
    }
}
