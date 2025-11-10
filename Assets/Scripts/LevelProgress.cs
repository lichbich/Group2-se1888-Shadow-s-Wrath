using UnityEngine;

public static class LevelProgress
{
    private const string LEVEL_KEY = "LevelUnlocked_";

    // Mở khóa level (ví dụ: sau khi thắng)
    public static void UnlockLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(LEVEL_KEY + levelIndex, 1);
        PlayerPrefs.Save();
    }

    // Kiểm tra level có mở chưa
    public static bool IsLevelUnlocked(int levelIndex)
    {
        // Level 1 luôn mở
        if (levelIndex == 1) return true;
        return PlayerPrefs.GetInt(LEVEL_KEY + levelIndex, 0) == 1;
    }
}
