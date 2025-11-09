using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseUIController : MonoBehaviour
{
    public void Retry()
    {
        PlayerPrefs.DeleteKey("CountVitality");
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("FinalMainMenu"); // thay bằng tên scene menu của bạn
    }
}
