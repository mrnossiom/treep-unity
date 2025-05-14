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
        [SerializeField] private TMP_InputField colorInput;

        [Header("Audio Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [SerializeField] private AudioMixer audioMixer;

        private void Start() {
            // Close button
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));

            // Username input
            usernameInput.text = Settings.Singleton.username;
            usernameInput.onEndEdit.AddListener(username => {
                Settings.Singleton.username = username;
                Settings.Singleton.SaveUserSettings();
            });

            // Volume sliders
            musicSlider.value = Settings.Singleton.musicVolume;
            sfxSlider.value = Settings.Singleton.sfxVolume;

            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // Apply on start
            ApplyVolume("MusicVolume", musicSlider.value);
            ApplyVolume("SFXVolume", sfxSlider.value);
        }

        private void OnMusicVolumeChanged(float value) {
            Settings.Singleton.SetMusicVolume(value);
            ApplyVolume("MusicVolume", value);
        }

        private void OnSFXVolumeChanged(float value) {
            Settings.Singleton.SetSFXVolume(value);
            ApplyVolume("SFXVolume", value);
        }

        private void ApplyVolume(string param, float value) {
            audioMixer.SetFloat(param, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        }
    }
}