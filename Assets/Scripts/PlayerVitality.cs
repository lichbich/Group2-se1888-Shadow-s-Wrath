using UnityEngine;
using TMPro;

public class PlayerVitality : MonoBehaviour
{
    public static int vitalityCount;
    public TextMeshProUGUI vitalityText;

    [Tooltip("Optional: assign the hearts controller that will show/hide filled hearts")]
    public PlayerHeartsController heartsController;

    private const int MaxVitality = 3;

    private void Start()
    {
        vitalityCount = MaxVitality;
        UpdateUI();
    }

    public static void AddVitality(int amount)
    {
        vitalityCount = Mathf.Clamp(vitalityCount + amount, 0, MaxVitality);
        UpdateUIStatic();
    }

    public static void ResetVitality()
    {
        vitalityCount = MaxVitality;
        UpdateUIStatic();
    }

    private void UpdateUI()
    {
        if (vitalityText != null)
            vitalityText.text = vitalityCount.ToString("00");

        if (heartsController != null)
            heartsController.SetHearts(vitalityCount);
    }

    private static void UpdateUIStatic()
    {
        var instance = FindObjectOfType<PlayerVitality>();
        if (instance != null)
        {
            if (instance.vitalityText != null)
                instance.vitalityText.text = vitalityCount.ToString("00");

            if (instance.heartsController != null)
                instance.heartsController.SetHearts(vitalityCount);
        }
    }
}
