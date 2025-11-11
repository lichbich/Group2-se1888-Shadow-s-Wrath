//using UnityEngine;
//using TMPro;

//public class ChestCollect : MonoBehaviour
//{
//    public TextMeshProUGUI countVitalityText;
//    private int countVitality = 0;

//    private void Start()
//    {
//        // Lấy giá trị CountVitality đã lưu (mặc định = 0)
//        countVitality = PlayerPrefs.GetInt("CountVitality", 0);
//        UpdateCountVitalityUI();
//    }

//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Chest"))
//        {
//            // Tăng điểm
//            countVitality++;

//            // Lưu điểm lại
//            PlayerPrefs.SetInt("CountVitality", countVitality);
//            PlayerPrefs.Save();

//            // Cập nhật UI
//            UpdateCountVitalityUI();

//            // Ẩn hoặc xóa chest
//            Destroy(collision.gameObject);
//        }
//    }


//    private void UpdateCountVitalityUI()
//    {
//        if (countVitalityText != null)
//        {
//            countVitalityText.text = countVitality.ToString("00"); // hiển thị dạng 01, 02,...
//        }
//    }
//}


using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ChestCollect : MonoBehaviour
{
    public TextMeshProUGUI countVitalityText;
    public GameObject closeDoor;   // cửa đóng
    public GameObject openDoor;    // cửa mở
    public GameObject key;         // object key (ẩn ban đầu)

    public GameObject collectTextObject;
    public GameObject takeKeyTextObject;

    public GameObject goToTextObject;

    private int countVitality = 0;
    private bool hasKey = false;

    private int chestsRequired = 5; // Số chest cần để hiện key

    private void Start()
    {
        // Lấy dữ liệu đã lưu (nếu có)
        countVitality = PlayerPrefs.GetInt("CountVitality", 0);
        hasKey = PlayerPrefs.GetInt("HasKey", 0) == 1;

        UpdateCountVitalityUI();
        UpdateKeyState();
        UpdateDoorState();

        if (takeKeyTextObject != null)
            takeKeyTextObject.SetActive(false);

        if (goToTextObject != null)
            goToTextObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Chest"))
        {
            //Phát âm thanh khi nhặt chest
            FindFirstObjectByType<AudioLevel1>()?.playChestSound();

            countVitality++;
            PlayerPrefs.SetInt("CountVitality", countVitality);
            PlayerPrefs.Save();

            UpdateCountVitalityUI();
            UpdateKeyState();
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Key"))
        {
            hasKey = true;
            PlayerPrefs.SetInt("HasKey", 1);
            PlayerPrefs.Save();

            UpdateDoorState();
            Destroy(collision.gameObject);
        }
    }

    private void UpdateCountVitalityUI()
    {
        if (countVitalityText != null)
            countVitalityText.text = countVitality.ToString("00");
    }

    private void UpdateKeyState()
    {
        bool showKeyAndTakeKeyText = (countVitality >= chestsRequired && !hasKey);

        if (key != null)
            key.SetActive(showKeyAndTakeKeyText);

        if (collectTextObject != null)
            collectTextObject.SetActive(!showKeyAndTakeKeyText);

        if (takeKeyTextObject != null)
            takeKeyTextObject.SetActive(showKeyAndTakeKeyText);

        if (goToTextObject != null)
            goToTextObject.SetActive(hasKey);
    }

    private void UpdateDoorState()
    {
        if (closeDoor != null && openDoor != null)
        {
            bool canOpen = hasKey;
            closeDoor.SetActive(!canOpen);
            openDoor.SetActive(canOpen);
        }
    }

    // 🧱 Gọi hàm này khi nhân vật chết hoặc restart level
    public void ResetProgress()
    {
        countVitality = 0;
        hasKey = false;

        PlayerPrefs.SetInt("CountVitality", 0);
        PlayerPrefs.SetInt("HasKey", 0);
        PlayerPrefs.Save();

        UpdateCountVitalityUI();
        UpdateKeyState();
        UpdateDoorState();
    }
}
