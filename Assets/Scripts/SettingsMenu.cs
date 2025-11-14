using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown resolutionDropdown; // Dropdown chọn Fullscreen/Windowed
    public Slider volumeSlider;             // Slider chỉnh âm lượng

    private void Start()
    {
        // Gán giá trị đã lưu trước đó
        int savedScreenMode = PlayerPrefs.GetInt("ScreenMode", 1); // 1 = Windowed, 0 = Fullscreen
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);

        resolutionDropdown.value = savedScreenMode;
        volumeSlider.value = savedVolume;

        // Áp dụng khi khởi động
        ApplyScreenMode(savedScreenMode);
        ApplyVolume(savedVolume);

        // Lắng nghe sự kiện người dùng thay đổi
        resolutionDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnScreenModeChanged(int index)
    {
        ApplyScreenMode(index);
        PlayerPrefs.SetInt("ScreenMode", index);
        PlayerPrefs.Save();
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();
    }

    private void ApplyScreenMode(int mode)
    {
        if (mode == 0)
        {
            // Full Screen
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            // Windowed
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("🎯 All progress has been reset!");
    }
    private void ApplyVolume(float volume)
    {
        AudioListener.volume = volume; // Chỉnh âm lượng toàn game
    }
}
