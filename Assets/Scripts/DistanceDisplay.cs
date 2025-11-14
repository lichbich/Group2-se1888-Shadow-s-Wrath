using UnityEngine;
using TMPro;

public class DistanceDisplay : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform destination;
    [SerializeField] private TextMeshProUGUI distanceText;

    void Update()
    {
        if (player == null || destination == null || distanceText == null) return;

        // chỉ đo khoảng cách ngang (trục X)
        float distance = Mathf.Abs(player.position.x - destination.position.x);

        if (distance <= 0.5f)
        {
            distanceText.text = "Arrived!";
        }
        else
        {
            distanceText.text = $"Distance: {distance:F1} m";
        }
    }
}
