using UnityEngine;
using TMPro;

public class UIManagerLevel2 : MonoBehaviour
{
    public TextMeshProUGUI pointEnd;

    public static UIManagerLevel2 instance;

    [SerializeField] private TMP_Text distanceText;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateDistance(float distance)
    {
        distanceText.text = "Distance: " + distance.ToString("0");
        if (pointEnd != null)
            pointEnd.text = distanceText.text;
    }
}
