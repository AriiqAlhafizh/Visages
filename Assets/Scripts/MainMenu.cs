using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("main");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
