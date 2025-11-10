using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public static int healthCount;
    public TextMeshProUGUI healthText;

    [Tooltip("Optional: assign the hearts controller that will show/hide filled hearts")]
    public PlayerHeartsController heartsController;

    private const int Maxhealth = 3;

    private void Start()
    {
        healthCount = Maxhealth;
        UpdateUI();
    }

    public static void Addhealth(int amount)
    {
        healthCount = Mathf.Clamp(healthCount + amount, 0, Maxhealth);
        UpdateUIStatic();
    }

    public static void Resethealth()
    {
        healthCount = Maxhealth;
        UpdateUIStatic();
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = healthCount.ToString("00");

        if (heartsController != null)
            heartsController.SetHearts(healthCount);
    }

    private static void UpdateUIStatic()
    {
        var instance = FindObjectOfType<PlayerHealth>();
        if (instance != null)
        {
            if (instance.healthText != null)
                instance.healthText.text = healthCount.ToString("00");

            if (instance.heartsController != null)
                instance.heartsController.SetHearts(healthCount);
        }
    }
}
