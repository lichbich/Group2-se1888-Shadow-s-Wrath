using UnityEngine;
using TMPro;

public class PlayerVitality : MonoBehaviour
{
    public static int vitalityCount;
    public TextMeshProUGUI vitalityText;

    private void Start()
    {
        vitalityCount = 0;
        UpdateUI();
    }

    public static void AddVitality(int amount)
    {
        vitalityCount += amount;
        UpdateUIStatic();
    }

    public static void ResetVitality()
    {
        vitalityCount = 0;
        UpdateUIStatic();
    }

    private void UpdateUI()
    {
        if (vitalityText != null)
            vitalityText.text = vitalityCount.ToString("00");
    }

    private static void UpdateUIStatic()
    {
        var instance = FindObjectOfType<PlayerVitality>();
        if (instance != null)
            instance.vitalityText.text = vitalityCount.ToString("00");
    }
}
