using Unity.VisualScripting;
using UnityEngine;

public class PlayerManagementLevel1 : MonoBehaviour
{
    private bool isDead = false;

    private Rigidbody2D rb;
    [SerializeField] private GameObject loseUI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            Debug.Log("Player touched Trap!");
            Die();
        }
    }

    void Die()
    {
        // Hiện giao diện thua
        if (loseUI != null)
        {
            loseUI.SetActive(true);
        }
        // Reset dữ liệu key + chest
        FindFirstObjectByType<ChestCollect>()?.ResetProgress();
        // Dừng game
        Time.timeScale = 0f;

        // Ẩn Player (tuỳ chọn)
        gameObject.SetActive(false);
    }

}
