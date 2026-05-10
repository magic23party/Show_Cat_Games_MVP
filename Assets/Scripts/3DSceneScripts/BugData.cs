using UnityEngine;

/// <summary>
/// Описание одного бага. Создаётся через Create -> Bugs -> Bug Data.
/// Каждый ноутбук в 3D-мире ссылается на свой BugData.
/// </summary>
[CreateAssetMenu(fileName = "Bug_New", menuName = "Bugs/Bug Data", order = 0)]
public class BugData : ScriptableObject
{
    [Tooltip("Уникальный идентификатор бага. ВАЖНО: должен быть уникальным среди всех багов.")]
    public string bugId;

    [Tooltip("Точное имя 2D-сцены (как в Build Settings). Регистр важен.")]
    public string sceneName;

    [Tooltip("Отображаемое название бага для UI-подсказки. Например: 'Сломанный фонарь'.")]
    public string displayName = "Bug";

    [TextArea(2, 4)]
    [Tooltip("Опциональное описание (можно использовать в будущем).")]
    public string description;
}
