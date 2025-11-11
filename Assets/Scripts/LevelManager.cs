using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    //public void UnlockNextLevel(int currentLevel)
    //{
    //    int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
    //    if (currentLevel >= unlocked)
    //    {
    //        PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
    //        PlayerPrefs.Save();
    //    }
    //}

    //public int GetUnlockedLevel()
    //{
    //    return PlayerPrefs.GetInt("UnlockedLevel", 1);
    //}
}
