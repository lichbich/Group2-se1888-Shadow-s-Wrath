using UnityEngine;

[DisallowMultipleComponent]
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Tooltip("UI prefab to show when the boss dies (instantiate or enable).")]
    public GameObject winUIPrefab;

    [Tooltip("UI prefab to show when the player loses (instantiate or enable).")]
    public GameObject loseUIPrefab;

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

    public void ShowWin()
    {
        if (winShown) return;
        winShown = true;

        if (winUIPrefab != null)
        {
            if (winInstance == null)
            {
                // Try to parent the instantiated UI under an existing Canvas so it renders properly.
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
            }
            else
            {
                winInstance.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("GameUIManager: winUIPrefab not assigned.");
        }
    }

    public void ShowLose()
    {
        if (loseShown) return;
        loseShown = true;

        if (loseUIPrefab != null)
        {
            if (loseInstance == null)
            {
                // Try to parent the instantiated UI under an existing Canvas so it renders properly.
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
            }
            else
            {
                loseInstance.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("GameUIManager: loseUIPrefab not assigned.");
        }
    }
}