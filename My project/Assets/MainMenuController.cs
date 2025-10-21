using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public void StartGame()
    {
        // Start from Level 1
        SceneManager.LoadScene("Level1");
    }

    public void OpenLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OpenLevel2()
    {
        SceneManager.LoadScene("Level2");
    }

    public void OpenLevel3()
    {
        SceneManager.LoadScene("Level3");
    }

    public void OpenIntroduction()
    {
        SceneManager.LoadScene("Introduction");
    }

    //public void QuitGame()
    //{
    //    Debug.Log("Game Quit!");
    //    Application.Quit();
    //}
}
