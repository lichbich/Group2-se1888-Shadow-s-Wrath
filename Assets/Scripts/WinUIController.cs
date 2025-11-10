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
        SceneManager.LoadScene("FinalMainMenu"); // đổi đúng tên scene menu
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int nextBuildIndex = currentBuildIndex + 1;

        // ✅ 1. Mở khóa level tiếp theo bằng PlayerPrefs (thay vì LevelManager.Instance)
        LevelProgress.UnlockLevel(nextBuildIndex);

        // ✅ 2. Kiểm tra xem có level tiếp theo trong Build Settings không
        if (nextBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextBuildIndex);
        }
        else
        {
            Debug.Log("🎉 Không còn level nào nữa! Quay lại menu...");
            SceneManager.LoadScene("FinalMainMenu");
        }
    }
}
