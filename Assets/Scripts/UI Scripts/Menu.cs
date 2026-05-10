using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public string loadScene;
    public GameObject player;

    public void LoadTargetScene()
    {
        SceneManager.LoadScene(loadScene);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");
        Application.Quit();
    }
}