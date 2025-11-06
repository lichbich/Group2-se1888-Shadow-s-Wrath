using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BossHealthUI : MonoBehaviour
{
    [Header("Fill Images (front = immediate, back = delayed)")]
    public Image frontFill;      // immediate fill (set instantly)
    public Image backFill;       // delayed fill that lerps down (damage "chip" effect)

    [Header("Texts")]
    public TextMeshProUGUI nameText;      // optional TMP boss name
    public Text legacyNameText;           // optional legacy UI Text fallback
    public TextMeshProUGUI percentText;   // optional percent display
    public Text legacyPercentText;

    [Header("Timing")]
    public float backLerpDelay = 0.12f;   // wait before backFill starts to move
    public float backLerpSpeed = 2.4f;    // speed of the delayed bar lerp
    public float fillSmoothTime = 0.06f;  // smooth time for immediate fill (small)
    public float visibleDurationAfterHit = 2.5f;
    public float fadeOutDuration = 0.6f;

    [Header("Entrance")]
    public RectTransform rootRect;        // rect to animate entrance (usually the same GameObject)
    public float entranceDuration = 0.45f;
    public Vector2 entranceOffset = new Vector2(0, 70); // start offset (y positive = below)
    public AnimationCurve entranceCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Hit Flash")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.08f;

    private CanvasGroup canvasGroup;
    private Coroutine hideCoroutine;
    private Coroutine backLerpCoroutine;
    private Coroutine frontFillCoroutine;
    private bool isVisible = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        if (frontFill != null) frontFill.fillAmount = 1f;
        if (backFill != null) backFill.fillAmount = 1f;
    }

    // Called by BossHealth to set the normalized health (0..1)
    public void SetHealth(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        // update texts
        UpdatePercentText(normalized);

        // immediate front fill - smooth small transition
        if (frontFill != null)
        {
            if (frontFillCoroutine != null) StopCoroutine(frontFillCoroutine);
            frontFillCoroutine = StartCoroutine(AnimateFrontFill(frontFill.fillAmount, normalized));
        }

        // delayed back fill (chip effect)
        if (backFill != null)
        {
            if (backLerpCoroutine != null) StopCoroutine(backLerpCoroutine);
            backLerpCoroutine = StartCoroutine(DelayedBackLerp(normalized));
        }

        // show UI while boss is taking damage
        ShowTemporarily();
        // optional flash
        if (frontFill != null) StartCoroutine(FlashFront());
    }

    public void SetBossName(string bossName)
    {
        if (nameText != null) nameText.text = bossName;
        if (legacyNameText != null) legacyNameText.text = bossName;
    }

    private IEnumerator AnimateFrontFill(float from, float to)
    {
        float t = 0f;
        float duration = Mathf.Max(fillSmoothTime, 0.01f);
        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.SmoothStep(from, to, t / duration);
            if (frontFill != null) frontFill.fillAmount = v;
            yield return null;
        }
        if (frontFill != null) frontFill.fillAmount = to;
        frontFillCoroutine = null;
    }

    private IEnumerator DelayedBackLerp(float target)
    {
        // If health increased (healing), set backFill immediately to target to avoid odd visuals
        if (backFill.fillAmount < target)
        {
            backFill.fillAmount = target;
            yield break;
        }

        yield return new WaitForSeconds(backLerpDelay);

        float t = 0f;
        float start = backFill.fillAmount;
        while (Mathf.Abs(backFill.fillAmount - target) > 0.001f)
        {
            backFill.fillAmount = Mathf.MoveTowards(backFill.fillAmount, target, backLerpSpeed * Time.deltaTime);
            yield return null;
        }
        backFill.fillAmount = target;
        backLerpCoroutine = null;
    }

    private void UpdatePercentText(float normalized)
    {
        int p = Mathf.RoundToInt(normalized * 100f);
        if (percentText != null) percentText.text = p + "%";
        if (legacyPercentText != null) legacyPercentText.text = p + "%";
    }

    private IEnumerator FlashFront()
    {
        if (frontFill == null) yield break;
        var original = frontFill.color;
        frontFill.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        frontFill.color = original;
    }

    // Show for visibleDurationAfterHit then fade out
    private void ShowTemporarily()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        ShowImmediate();
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleDurationAfterHit);

        float t = 0f;
        float start = canvasGroup.alpha;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        isVisible = false;
        hideCoroutine = null;
    }

    public void ShowImmediate()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        canvasGroup.alpha = 1f;
        isVisible = true;
    }

    // Entrance animation: slide from offset into position and show
    public void ShowEntrance()
    {
        if (rootRect == null) rootRect = GetComponent<RectTransform>();
        if (rootRect == null) return;

        // stop any running coroutines that affect transform or alpha
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        StartCoroutine(EntranceCoroutine());
    }

    private IEnumerator EntranceCoroutine()
    {
        ShowImmediate();

        Vector2 originalAnchored = rootRect.anchoredPosition;
        rootRect.anchoredPosition = originalAnchored - entranceOffset;

        float t = 0f;
        while (t < entranceDuration)
        {
            t += Time.deltaTime;
            float a = entranceCurve.Evaluate(Mathf.Clamp01(t / entranceDuration));
            rootRect.anchoredPosition = Vector2.Lerp(originalAnchored - entranceOffset, originalAnchored, a);
            yield return null;
        }
        rootRect.anchoredPosition = originalAnchored;
    }
}