using UnityEngine;

public class PlayerHeartsController : MonoBehaviour
{
    [Tooltip("Filled child of heart 1 (represents HP 1)")]
    public GameObject heartFilled1;

    [Tooltip("Filled child of heart 2 (represents HP 2)")]
    public GameObject heartFilled2;

    [Tooltip("Filled child of heart 3 (represents HP 3)")]
    public GameObject heartFilled3;

    private const int MaxHearts = 3;

    /// <summary>
    /// Set the visible filled hearts based on current HP (0..3).
    /// Heart 1 corresponds to the left-most (HP 1) and so on.
    /// </summary>
    public void SetHearts(int currentHp)
    {
        int hp = Mathf.Clamp(currentHp, 0, MaxHearts);
        if (heartFilled1 != null) heartFilled1.SetActive(hp >= 1);
        if (heartFilled2 != null) heartFilled2.SetActive(hp >= 2);
        if (heartFilled3 != null) heartFilled3.SetActive(hp >= 3);
    }
}