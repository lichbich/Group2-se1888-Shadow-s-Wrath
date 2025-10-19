using UnityEngine;

public class CameraAutoMove : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public float startX = -38f;       // Vị trí bắt đầu
    public float endX = 928.7f;       // Vị trí kết thúc
    public float yPos = 1.9f;         // Giữ nguyên tọa độ Y
    public float zPos = -10f;         // Giữ nguyên tọa độ Z (camera 2D)
    public float speed = 5f;          // Tốc độ di chuyển (unit/giây)
    public bool autoStart = true;     // Có tự động di chuyển khi game bắt đầu không

    private bool isMoving;

    void Start()
    {
        // Đặt camera ở tọa độ ban đầu
        transform.position = new Vector3(startX, yPos, zPos);

        // Bắt đầu di chuyển nếu bật autoStart
        isMoving = autoStart;
    }

    void Update()
    {
        if (!isMoving) return;

        // Di chuyển camera theo trục X
        float newX = transform.position.x + speed * Time.deltaTime;

        // Giới hạn không vượt quá endX
        if (newX >= endX)
        {
            newX = endX;
            isMoving = false; // Dừng lại khi đến cuối
        }

        transform.position = new Vector3(newX, yPos, zPos);
    }

    // Hàm này để bạn có thể bật lại di chuyển khi cần (ví dụ khi player chạm trigger)
    public void StartMoving()
    {
        isMoving = true;
    }
}
