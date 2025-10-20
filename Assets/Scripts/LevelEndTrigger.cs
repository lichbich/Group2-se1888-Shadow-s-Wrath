using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    public GameObject winUI; // gán UI You Win ở đây

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hiện UI thắng
            winUI.SetActive(true);

            // Dừng game (nếu muốn)
            Time.timeScale = 0f;
        }
    }

}
