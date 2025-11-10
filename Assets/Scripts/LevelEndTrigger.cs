using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    public GameObject winUI; // gán UI You Win ở đây
    [SerializeField] private AudioClip winClip;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hiện UI thắng
            winUI.SetActive(true);

            // Phát âm thanh thắng cuộc
            FindFirstObjectByType<AudioLevel2>()?.playWinSound();


            // Dừng game (nếu muốn)
            Time.timeScale = 0f;
        }
    }

}
