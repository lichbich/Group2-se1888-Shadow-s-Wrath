using UnityEngine;
using System.Collections;

public class CameraAutoMove : MonoBehaviour
{
    [SerializeField] private GameObject player;

    void Update()
    {
        if (player != null)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = player.transform.position.x + 3f; // Giữ khoảng cách cố định phía trước người chơi
            newPosition.z = transform.position.z; // Giữ nguyên vị trí y của camera
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }
    }
}
