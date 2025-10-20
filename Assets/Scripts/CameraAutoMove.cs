using UnityEngine;
using System.Collections;

public class CameraAutoMove : MonoBehaviour
{
    // Nhân vật cần theo dõi
    public Transform target;

    // Vị trí Y cố định cho camera
    private float fixedY;

    // Khoảng cách cố định theo trục Z (thường cho camera 2D)
    public float zOffset = -10f;

    // Tốc độ làm mượt chuyển động
    public float smoothSpeed = 5f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Chưa gán target cho camera!");
            return;
        }
        // Lưu trữ vị trí Y ban đầu của camera
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Vị trí `newPosition` chỉ cập nhật X, giữ Y và Z cố định
            Vector3 newPosition = new Vector3(target.position.x, fixedY, target.position.z + zOffset);

            // Làm mượt chuyển động của camera
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * smoothSpeed);
        }
    }
}
