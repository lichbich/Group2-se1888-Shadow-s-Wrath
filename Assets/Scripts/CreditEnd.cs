using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditEnd : MonoBehaviour
{
    public float duration = 20f; // thời gian chạy credit

    void Start()
    {
        Invoke(nameof(ReturnToMenu), duration);
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        // Hoặc Application.Quit() nếu bạn muốn thoát game
    }
}
