using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDoorTrigger : MonoBehaviour
{
    public GameObject WinUI;         // UI khi thắng
    public GameObject PointUI;       // UI điểm số
    public string nextLevelName = "Level2";  // tên scene kế tiếp

    private void Start()
    {
        WinUI.SetActive(false);
        PointUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Phát âm thanh thắng cuộc
            WinUI.SetActive(true);
            PointUI.SetActive(true);
            FindFirstObjectByType<AudioLevel1>()?.playWinSound();

            // Chuyển sang level 2 sau 2 giây (có thể chỉnh)
            Invoke(nameof(LoadNextLevel), 10f);
        }
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}
