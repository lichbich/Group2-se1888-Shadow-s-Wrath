using Unity.VisualScripting;
using UnityEngine;

public class PlayerManagementLevel1 : MonoBehaviour
{
    private bool isDead = false;

    private Rigidbody2D rb;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private AudioClip loseClip;
    


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
        //Phát thanh khi thua 
        FindFirstObjectByType<AudioLevel1>()?.playLoseSound();

        // Reset dữ liệu key + chest
        FindFirstObjectByType<ChestCollect>()?.ResetProgress();
        // Dừng game
        Time.timeScale = 0f;

        // Ẩn Player (tuỳ chọn)
        gameObject.SetActive(false);
    }

}
