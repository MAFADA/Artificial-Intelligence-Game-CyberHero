using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;
    [SerializeField] Toggle muteToggle;
    [SerializeField] Slider bgmSldier;
    [SerializeField] Slider sfxSldier;
    [SerializeField] TMP_Text bgmVolText;
    [SerializeField] TMP_Text sfxVolText;

    private void OnEnable()
    {
        muteToggle.isOn = audioManager.IsMute;
        bgmSldier.value = audioManager.BgmVolume;
        sfxSldier.value = audioManager.SfxVolume;
        SetBgmVolText(bgmSldier.value);
        SetSfxVolText(sfxSldier.value);
    }

    public void SetBgmVolText(float value)
    {
        bgmVolText.text = Mathf.RoundToInt(value * 100).ToString();
    }

    public void SetSfxVolText(float value)
    {
        sfxVolText.text = Mathf.RoundToInt(value * 100).ToString();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("bgmVolume", audioManager.BgmVolume);
        PlayerPrefs.SetFloat("sfxVolume", audioManager.SfxVolume);
        PlayerPrefs.SetInt("bgmMute", audioManager.IsMute ? 1 : 0);
    }
}
