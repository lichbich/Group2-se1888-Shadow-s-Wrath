using UnityEngine;
using TMPro;

public class SyncScoreToTMP : MonoBehaviour
{
    [Tooltip("If empty, the component will try to use the TextMeshProUGUI on this GameObject or its children.")]
    public TextMeshProUGUI targetText;

    [Tooltip("Prefix shown before the numeric score.")]
    public string prefix = "Score: ";

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();

        if (targetText == null)
            return;

        if (GameUIManager.Instance != null)
        {
            // set initial value
            targetText.text = prefix + GameUIManager.Instance.GetScore().ToString();
            // subscribe for live updates
            GameUIManager.Instance.ScoreChanged += OnScoreChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameUIManager.Instance != null)
            GameUIManager.Instance.ScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int newScore)
    {
        if (targetText != null)
            targetText.text = prefix + newScore.ToString();
    }
}