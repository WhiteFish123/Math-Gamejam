using UnityEngine;
using UnityEngine.UI;
using GameCore;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    void OnEnable()
    {
        if (AudioManager.instance != null)
        {
            bgmSlider.value = AudioManager.instance.GetBGMVolume();
            sfxSlider.value = AudioManager.instance.GetSFXVolume();
        }

        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    void OnDisable()
    {
        bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
    }

    void OnBGMChanged(float value)
    {
        AudioManager.instance?.SetBGMVolume(value);
    }

    void OnSFXChanged(float value)
    {
        AudioManager.instance?.SetSFXVolume(value);
    }

    public void OnContinueClicked()
    {
        Debug.Log("[SettingsUI] 继续游戏按钮被点击");
        Debug.Log($"[SettingsUI] GameManager.instance = {(GameManager.instance != null ? "存在" : "NULL")}");
        GameManager.instance?.ToggleSettings();
    }

    public void OnMainMenuClicked()
    {
        Debug.Log("[SettingsUI] 返回主界面按钮被点击");
        Debug.Log($"[SettingsUI] LevelManager.instance = {(LevelManager.instance != null ? "存在" : "NULL")}");
        LevelManager.instance?.LoadScene("MainMenu");
    }

    public void OnExitClicked()
    {
        Debug.Log("[SettingsUI] 退出游戏按钮被点击");
        LevelManager.instance?.ExitGame();
    }
}