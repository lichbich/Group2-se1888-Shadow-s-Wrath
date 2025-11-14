using UnityEngine;
using UnityEngine.SceneManagement; // cần để load scene

public class PauseController : MonoBehaviour
{
    public GameObject pauseButton;
    public GameObject resumeButton;
    public GameObject pausePopup; // <-- thêm cái popup UI panel

    private bool isPaused = false;

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        pauseButton.SetActive(false);
        resumeButton.SetActive(true);
        pausePopup.SetActive(true); // hiện popup
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        pauseButton.SetActive(true);
        resumeButton.SetActive(false);
        pausePopup.SetActive(false); // ẩn popup
    }

    public void BackToMainMenu()
    {
        // Đặt lại timeScale để tránh lỗi game bị dừng ở Main Menu
        Time.timeScale = 1f;
        SceneManager.LoadScene("FinalMainMenu"); // thay bằng tên scene của bạn
    }
}
