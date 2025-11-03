using UnityEngine;
using TMPro;

public class ChestCollect : MonoBehaviour
{
    public TextMeshProUGUI countVitalityText;
    private int countVitality = 0;

    private void Start()
    {
        // Lấy giá trị CountVitality đã lưu (mặc định = 0)
        countVitality = PlayerPrefs.GetInt("CountVitality", 0);
        UpdateCountVitalityUI();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Chest"))
        {
            // Tăng điểm
            countVitality++;

            // Lưu điểm lại
            PlayerPrefs.SetInt("CountVitality", countVitality);
            PlayerPrefs.Save();

            // Cập nhật UI
            UpdateCountVitalityUI();

            // Ẩn hoặc xóa chest
            Destroy(collision.gameObject);
        }
    }


    private void UpdateCountVitalityUI()
    {
        if (countVitalityText != null)
        {
            countVitalityText.text = countVitality.ToString("00"); // hiển thị dạng 01, 02,...
        }
    }
}
