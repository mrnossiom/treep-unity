using TMPro;
using Treep.State;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Treep.Interface {
    public class SettingsMenu : MonoBehaviour {
        [Header("UI Elements")]
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_InputField usernameInput;

        [Header("Audio Sliders")]
        [SerializeField] private Slider mainSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private void Start() {
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            
            usernameInput.text = Settings.Singleton.username;
            usernameInput.onEndEdit.AddListener(username => {
                Settings.Singleton.username = username;
                Settings.Singleton.SaveUserSettings();
            });
            
            musicSlider.value = Settings.Singleton.GetMusicVolumePercent();
            sfxSlider.value = Settings.Singleton.GetSfxVolumePercent();
            this.mainSlider.value = Settings.Singleton.GetMasterVolumePercent();

            musicSlider.onValueChanged.AddListener(Settings.Singleton.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(Settings.Singleton.SetSfxVolume);
            this.mainSlider.onValueChanged.AddListener(Settings.Singleton.SetMasterVolume);
        }
    }
}