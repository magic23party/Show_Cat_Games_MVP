using TMPro;
using UnityEngine;

/// <summary>
/// UI-подсказка "Press E to fix [имя бага]".
/// Вешается на отдельный Canvas с TextMeshPro-текстом по центру экрана (или внизу).
/// Синглтон — обращайся через InteractionPrompt.Instance.
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    [SerializeField] private GameObject root;       // корневой объект, который включается/выключается
    [SerializeField] private TMP_Text label;        // сам текст

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Hide();
    }

    public void Show(string text)
    {
        if (label != null) label.text = text;
        if (root != null) root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
