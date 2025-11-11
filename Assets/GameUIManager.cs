using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Tooltip("UI prefab to show when the boss dies (instantiate or enable).")]
    public GameObject winUIPrefab;

    [Tooltip("UI prefab to show when the player loses (instantiate or enable).")]
    public GameObject loseUIPrefab;

    [Header("Timing (seconds)")]
    [Tooltip("Default delay before showing the Win UI when calling parameterless ShowWin(). Uses scaled time unless 'useRealtimeForDelays' is checked.")]
    public float defaultWinDelay = 0f;
    [Tooltip("Default delay before showing the Lose UI when calling parameterless ShowLose(). Uses scaled time unless 'useRealtimeForDelays' is checked.")]
    public float defaultLoseDelay = 0f;
    [Tooltip("If true, delays use unscaled realtime (WaitForSecondsRealtime) so UI still appears when Time.timeScale == 0.")]
    public bool useRealtimeForDelays = false;

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

        if (winUIPrefab == null)
        {
            Debug.LogWarning("GameUIManager: winUIPrefab not assigned.");
            return;
        }

        // If user assigned a scene object (not a prefab asset) use it directly
        if (winInstance == null)
        {
            if (winUIPrefab.scene.IsValid())
            {
                winInstance = winUIPrefab;
                EnsureUIOnTop(winInstance);
                winInstance.SetActive(true);
            }
            else
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    winInstance = Instantiate(winUIPrefab, canvas.transform, false);
                }
                else
                {
                    // fallback if no Canvas found in scene
                    winInstance = Instantiate(winUIPrefab);
                }

                // Make sure instantiated UI is active (prefab might be created inactive)
                if (winInstance != null && !winInstance.activeSelf)
                    winInstance.SetActive(true);

                EnsureUIOnTop(winInstance);
            }
        }
        else
        {
            winInstance.SetActive(true);
            EnsureUIOnTop(winInstance);
        }
    }

    // Parameterless calls now use configurable defaults.
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

        if (loseUIPrefab == null)
        {
            Debug.LogWarning("GameUIManager: loseUIPrefab not assigned.");
            return;
        }

        // Support both: a prefab asset (instantiate) or an existing scene object (enable)
        if (loseInstance == null)
        {
            if (loseUIPrefab.scene.IsValid())
            {
                // loseUIPrefab is a scene object reference -> enable it
                loseInstance = loseUIPrefab;
                EnsureUIOnTop(loseInstance);
                loseInstance.SetActive(true);
            }
            else
            {
                // loseUIPrefab is a prefab asset -> instantiate under Canvas if available
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    loseInstance = Instantiate(loseUIPrefab, canvas.transform, false);
                }
                else
                {
                    // fallback if no Canvas found in scene
                    loseInstance = Instantiate(loseUIPrefab);
                }

                // Make sure instantiated UI is active (prefab might be created inactive)
                if (loseInstance != null && !loseInstance.activeSelf)
                    loseInstance.SetActive(true);

                EnsureUIOnTop(loseInstance);
            }
        }
        else
        {
            loseInstance.SetActive(true);
            EnsureUIOnTop(loseInstance);
        }
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