using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManagementLevel1 : MonoBehaviour
{
    private bool isDead = false;

    private Rigidbody2D rb;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private AudioClip loseClip;
    public GameObject PointUI;
    public TextMeshProUGUI pointEndText;



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
        Debug.Log("Die() called");

        // lấy instance ChestCollect (cẩn thận null)
        var chest = FindFirstObjectByType<ChestCollect>();
        if (chest == null)
        {
            Debug.LogWarning("ChestCollect not found!");
        }
        else
        {
            Debug.Log("ChestCollect.GetCountVitality() = " + chest.GetCountVitality());
        }

        int prefsValue = PlayerPrefs.GetInt("CountVitality", -1);
        Debug.Log("PlayerPrefs CountVitality = " + prefsValue);

        //=====================

        // Hiện giao diện thua
        if (loseUI != null)
        {
            loseUI.SetActive(true);
        }
        //Phát thanh khi thua 
        FindFirstObjectByType<AudioLevel1>()?.playLoseSound();

        // LẤY ĐIỂM từ ChestCollect trước khi reset
        int finalScore = FindFirstObjectByType<ChestCollect>().GetCountVitality();
        if (pointEndText != null)
        {
            pointEndText.text = chest.GetCountVitality().ToString("00");
        }

        // Reset dữ liệu
        FindFirstObjectByType<ChestCollect>()?.ResetProgress();

        // Dừng game
        Time.timeScale = 0f;

        // Ẩn Player (tuỳ chọn)
        gameObject.SetActive(false);
        PointUI.SetActive(true);
    }

}
