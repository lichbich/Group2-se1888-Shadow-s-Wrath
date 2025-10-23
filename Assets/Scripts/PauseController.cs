using UnityEngine;

public class PauseController : MonoBehaviour
{
    public GameObject pauseButton;
    public GameObject resumeButton;

    private bool isPaused = false;

    public void PauseGame()
    {
        Time.timeScale = 0f; // Dừng game
        isPaused = true;
        pauseButton.SetActive(false);
        resumeButton.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Tiếp tục game
        isPaused = false;
        pauseButton.SetActive(true);
        resumeButton.SetActive(false);
    }
}
