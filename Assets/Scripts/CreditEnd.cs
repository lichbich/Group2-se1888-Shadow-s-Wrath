using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditEnd : MonoBehaviour
{
    public float duration = 20f; // thời gian chạy credit

    // Biến cờ để đảm bảo chỉ gọi hàm thoát 1 lần
    private bool isExiting = false;

    void Start()
    {
        Invoke(nameof(ReturnToMenu), duration);
    }

    void Update()
    {
        // Nếu người dùng click VÀ chúng ta chưa bắt đầu quá trình thoát
        if (Input.GetMouseButtonDown(0) && !isExiting)
        {
            // 1. Đánh dấu là đang thoát
            isExiting = true;

            // 2. Hủy lệnh Invoke 20s ban đầu
            CancelInvoke(nameof(ReturnToMenu));

            // 3. Hẹn giờ 0.5 giây, SAU ĐÓ mới gọi ReturnToMenu
            Invoke(nameof(ReturnToMenu), 0.3f);
        }
    }

    void ReturnToMenu()
    {
        // Vì isExiting đã được set, code này sẽ chỉ chạy 1 lần
        SceneManager.LoadScene("FinalMainMenu");
    }
}