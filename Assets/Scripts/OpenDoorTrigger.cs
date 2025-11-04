using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDoorTrigger : MonoBehaviour
{
    public GameObject WinUI;          // UI khi thắng
    public string nextLevelName = "Level2";  // tên scene kế tiếp

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
                WinUI.SetActive(true);

            // Chuyển sang level 2 sau 2 giây (có thể chỉnh)
            Invoke(nameof(LoadNextLevel), 2f);
        }
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}
