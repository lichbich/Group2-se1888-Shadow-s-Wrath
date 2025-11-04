using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUIController : MonoBehaviour
{
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("FinalMainMenu"); // thay bằng tên scene menu của bạn
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        LevelManager.Instance.UnlockNextLevel(currentLevel); // ✅ Mở khóa level tiếp theo

        SceneManager.LoadScene(currentLevel + 1); // sang level kế
    }
}
