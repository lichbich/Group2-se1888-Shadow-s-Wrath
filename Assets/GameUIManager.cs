using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Tooltip("UI prefab to show when the boss dies (instantiate or enable).")]
    public GameObject winUIPrefab;

    [Tooltip("UI prefab to show when the player loses (instantiate or enable).")]
    public GameObject loseUIPrefab;

    [Header("Per-level UI (optional)")]
    [Tooltip("Optional: indexed list of Win UI prefabs per level. Index 0 => level 1, index 1 => level 2, etc.")]
    public GameObject[] winUIPrefabsByLevel;
    [Tooltip("Optional: indexed list of Lose UI prefabs per level. Index 0 => level 1, index 1 => level 2, etc.")]
    public GameObject[] loseUIPrefabsByLevel;

    [Header("Timing (seconds)")]
    [Tooltip("Default delay before showing the Win UI when calling parameterless ShowWin(). Uses scaled time unless 'useRealtimeForDelays' is checked.")]
    public float defaultWinDelay = 0f;
    [Tooltip("Default delay before showing the Lose UI when calling parameterless ShowLose(). Uses scaled time unless 'useRealtimeForDelays' is checked.")]
    public float defaultLoseDelay = 0f;
    [Tooltip("If true, delays use unscaled realtime (WaitForSecondsRealtime) so UI still appears when Time.timeScale == 0.")]
    public bool useRealtimeForDelays = false;

    [Header("Score (optional)")]
    [Tooltip("Optional TMP Text to show the live score. Assign a TextMeshPro - Text (UI) component in the Inspector.")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Starting score")]
    public int startingScore = 0;

    private int score = 0;

    // Event fired whenever score changes
    public event Action<int> ScoreChanged;

    private GameObject winInstance;
    private GameObject loseInstance;
    private bool winShown = false;
    private bool loseShown = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameUIManager detected — destroying duplicate GameObject.");
            // Destroy the entire duplicate GameObject to avoid leftover components/objects.
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // initialize score
        score = startingScore;
        UpdateScoreUI();
    }

    // Score API
    public void AddScore(int points)
    {
        if (points == 0) return;
        score += points;
        UpdateScoreUI();
        ScoreChanged?.Invoke(score);
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        UpdateScoreUI();
        ScoreChanged?.Invoke(score);
    }

    public int GetScore() => score;

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    // Inject current score into a newly created UI prefab instance (searches child named defaultChildName first,
    // otherwise uses first TMP child). Useful for one-time copy (e.g. win screen).
    public void InjectScoreIntoUI(GameObject uiRoot, string defaultChildName = "FinalScoreText", string prefix = "Score: ")
    {
        if (uiRoot == null) return;

        TextMeshProUGUI target = null;

        // try find by name
        var t = uiRoot.transform.Find(defaultChildName);
        if (t != null) target = t.GetComponent<TextMeshProUGUI>();

        // fallback: find first TMP child anywhere
        if (target == null) target = uiRoot.GetComponentInChildren<TextMeshProUGUI>();

        if (target != null)
            target.text = prefix + score.ToString();
    }

    // Parameterless calls now use configurable defaults.
    public void ShowWin()
    {
        ShowWin(defaultWinDelay);
    }

    // New: show with optional delay (seconds)
    public void ShowWin(float delaySeconds)
    {
        if (delaySeconds <= 0f)
        {
            ShowWinNow();
        }
        else
        {
            StartCoroutine(ShowWinAfter(delaySeconds));
        }
    }

    private IEnumerator ShowWinAfter(float seconds)
    {
        if (useRealtimeForDelays)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);

        ShowWinNow();
    }

    private void ShowWinNow()
    {
        if (winShown) return;
        winShown = true;

        GameObject prefabToUse = GetPrefabForCurrentLevel(winUIPrefabsByLevel, winUIPrefab);

        if (prefabToUse == null)
        {
            Debug.LogWarning("GameUIManager: winUIPrefab not assigned (and no per-level fallback).");
            return;
        }

        // If user assigned a scene object (not a prefab asset) use it directly
        if (winInstance == null)
        {
            if (prefabToUse.scene.IsValid())
            {
                winInstance = prefabToUse;
                EnsureUIOnTop(winInstance);
                winInstance.SetActive(true);
                InjectScoreIntoUI(winInstance);
            }
            else
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    winInstance = Instantiate(prefabToUse, canvas.transform, false);
                }
                else
                {
                    // fallback if no Canvas found in scene
                    winInstance = Instantiate(prefabToUse);
                }

                // Make sure instantiated UI is active (prefab might be created inactive)
                if (winInstance != null && !winInstance.activeSelf)
                    winInstance.SetActive(true);

                EnsureUIOnTop(winInstance);
                InjectScoreIntoUI(winInstance);
            }
        }
        else
        {
            winInstance.SetActive(true);
            EnsureUIOnTop(winInstance);
            InjectScoreIntoUI(winInstance);
        }
    }

    // Overload: show Win UI for a specific level (levelNumber is 1-based). Uses default delay.
    public void ShowWinForLevel(int levelNumber)
    {
        ShowWinForLevel(levelNumber, defaultWinDelay);
    }

    // Overload: show Win UI for a specific level with delay
    public void ShowWinForLevel(int levelNumber, float delaySeconds)
    {
        if (delaySeconds <= 0f)
        {
            ShowWinNowForLevel(levelNumber);
        }
        else
        {
            StartCoroutine(ShowWinForLevelAfter(levelNumber, delaySeconds));
        }
    }

    private IEnumerator ShowWinForLevelAfter(int levelNumber, float seconds)
    {
        if (useRealtimeForDelays)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);

        ShowWinNowForLevel(levelNumber);
    }

    private void ShowWinNowForLevel(int levelNumber)
    {
        if (winShown) return;
        winShown = true;

        GameObject prefabToUse = GetPrefabForLevel(winUIPrefabsByLevel, winUIPrefab, levelNumber);

        if (prefabToUse == null)
        {
            Debug.LogWarning("GameUIManager: win UI prefab for level not assigned and no default fallback available.");
            return;
        }

        if (winInstance == null)
        {
            if (prefabToUse.scene.IsValid())
            {
                winInstance = prefabToUse;
                EnsureUIOnTop(winInstance);
                winInstance.SetActive(true);
                InjectScoreIntoUI(winInstance);
            }
            else
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    winInstance = Instantiate(prefabToUse, canvas.transform, false);
                }
                else
                {
                    winInstance = Instantiate(prefabToUse);
                }

                if (winInstance != null && !winInstance.activeSelf)
                    winInstance.SetActive(true);

                EnsureUIOnTop(winInstance);
                InjectScoreIntoUI(winInstance);
            }
        }
        else
        {
            winInstance.SetActive(true);
            EnsureUIOnTop(winInstance);
            InjectScoreIntoUI(winInstance);
        }
    }

    // Parameterless calls use configurable defaults.
    public void ShowLose()
    {
        ShowLose(defaultLoseDelay);
    }

    // New: show with optional delay (seconds)
    public void ShowLose(float delaySeconds)
    {
        if (delaySeconds <= 0f)
        {
            ShowLoseNow();
        }
        else
        {
            StartCoroutine(ShowLoseAfter(delaySeconds));
        }
    }

    private IEnumerator ShowLoseAfter(float seconds)
    {
        if (useRealtimeForDelays)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);

        ShowLoseNow();
    }

    private void ShowLoseNow()
    {
        if (loseShown) return;
        loseShown = true;

        GameObject prefabToUse = GetPrefabForCurrentLevel(loseUIPrefabsByLevel, loseUIPrefab);

        if (prefabToUse == null)
        {
            Debug.LogWarning("GameUIManager: loseUIPrefab not assigned (and no per-level fallback).");
            return;
        }

        // Support both: a prefab asset (instantiate) or an existing scene object (enable)
        if (loseInstance == null)
        {
            if (prefabToUse.scene.IsValid())
            {
                // loseUIPrefab is a scene object reference -> enable it
                loseInstance = prefabToUse;
                EnsureUIOnTop(loseInstance);
                loseInstance.SetActive(true);
                InjectScoreIntoUI(loseInstance);
            }
            else
            {
                // loseUIPrefab is a prefab asset -> instantiate under Canvas if available
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    loseInstance = Instantiate(prefabToUse, canvas.transform, false);
                }
                else
                {
                    // fallback if no Canvas found in scene
                    loseInstance = Instantiate(prefabToUse);
                }

                // Make sure instantiated UI is active (prefab might be created inactive)
                if (loseInstance != null && !loseInstance.activeSelf)
                    loseInstance.SetActive(true);

                EnsureUIOnTop(loseInstance);
                InjectScoreIntoUI(loseInstance);
            }
        }
        else
        {
            loseInstance.SetActive(true);
            EnsureUIOnTop(loseInstance);
            InjectScoreIntoUI(loseInstance);
        }
    }

    // Overload: show Lose UI for specific level (1-based)
    public void ShowLoseForLevel(int levelNumber)
    {
        ShowLoseForLevel(levelNumber, defaultLoseDelay);
    }

    public void ShowLoseForLevel(int levelNumber, float delaySeconds)
    {
        if (delaySeconds <= 0f)
        {
            ShowLoseNowForLevel(levelNumber);
        }
        else
        {
            StartCoroutine(ShowLoseForLevelAfter(levelNumber, delaySeconds));
        }
    }

    private IEnumerator ShowLoseForLevelAfter(int levelNumber, float seconds)
    {
        if (useRealtimeForDelays)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);

        ShowLoseNowForLevel(levelNumber);
    }

    private void ShowLoseNowForLevel(int levelNumber)
    {
        if (loseShown) return;
        loseShown = true;

        GameObject prefabToUse = GetPrefabForLevel(loseUIPrefabsByLevel, loseUIPrefab, levelNumber);

        if (prefabToUse == null)
        {
            Debug.LogWarning("GameUIManager: lose UI prefab for level not assigned and no default fallback available.");
            return;
        }

        if (loseInstance == null)
        {
            if (prefabToUse.scene.IsValid())
            {
                loseInstance = prefabToUse;
                EnsureUIOnTop(loseInstance);
                loseInstance.SetActive(true);
                InjectScoreIntoUI(loseInstance);
            }
            else
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    loseInstance = Instantiate(prefabToUse, canvas.transform, false);
                }
                else
                {
                    loseInstance = Instantiate(prefabToUse);
                }

                if (loseInstance != null && !loseInstance.activeSelf)
                    loseInstance.SetActive(true);

                EnsureUIOnTop(loseInstance);
                InjectScoreIntoUI(loseInstance);
            }
        }
        else
        {
            loseInstance.SetActive(true);
            EnsureUIOnTop(loseInstance);
            InjectScoreIntoUI(loseInstance);
        }
    }

    // Helper: choose prefab for the active/current level using per-level array if available.
    // If the array is null/empty or the index is out-of-range, returns the fallback defaultPrefab.
    private GameObject GetPrefabForCurrentLevel(GameObject[] perLevelArray, GameObject defaultPrefab)
    {
        // try to infer current level from active scene build index (1-based)
        int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        // Treat buildIndex as levelNumber (1-based is conventional, but buildIndex may be 0-based).
        // We'll map directly: index 0 => level 0; caller arrays usually set accordingly.
        return GetPrefabForLevel(perLevelArray, defaultPrefab, currentLevel);
    }

    private GameObject GetPrefabForLevel(GameObject[] perLevelArray, GameObject defaultPrefab, int levelNumber)
    {
        if (perLevelArray != null && perLevelArray.Length > 0)
        {
            int idx = levelNumber; // use levelNumber directly as index to allow editor control
            if (idx >= 0 && idx < perLevelArray.Length && perLevelArray[idx] != null)
                return perLevelArray[idx];
        }
        return defaultPrefab;
    }

    // Ensure the UI/gameobject appears on top of other UI:
    // - If it's parented under a Canvas, move it to the end of the sibling list
    // - If it's a standalone Canvas (or not under a Canvas), give it overrideSorting with high sortingOrder
    private void EnsureUIOnTop(GameObject uiGO)
    {
        if (uiGO == null) return;

        // if already under a Canvas in scene, push it to last sibling so it renders above siblings
        Canvas parentCanvas = uiGO.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            // If the instance itself is a top-level (has its own Canvas), ensure overrideSorting
            var selfCanvas = uiGO.GetComponent<Canvas>();
            if (selfCanvas != null)
            {
                selfCanvas.overrideSorting = true;
                // choose a high but safe order; you can tweak this value in inspector by adding a Canvas on the prefab.
                selfCanvas.sortingOrder = 1000;
            }

            // Move root UI object to the end of the canvas's children so it draws last
            Transform root = uiGO.transform;
            // find the immediate child of the canvas (in case the prefab is nested)
            while (root.parent != null && root.parent != parentCanvas.transform)
                root = root.parent;
            root.SetAsLastSibling();

            // Force a canvas update to avoid draw-order glitches
            Canvas.ForceUpdateCanvases();
            return;
        }

        // Not under any canvas: add a Canvas component so it can be sorted on top
        var addedCanvas = uiGO.GetComponent<Canvas>();
        if (addedCanvas == null)
            addedCanvas = uiGO.AddComponent<Canvas>();

        addedCanvas.overrideSorting = true;
        addedCanvas.sortingOrder = 1000;

        // Ensure raycasts still work if the prefab needs them
        if (uiGO.GetComponent<GraphicRaycaster>() == null)
            uiGO.AddComponent<GraphicRaycaster>();
    }
}