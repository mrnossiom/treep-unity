using UnityEngine;
using UnityEngine.Audio;

namespace Treep.State {
    public class Settings : MonoBehaviour {
        public static Settings Singleton;

        [Header("User Settings")]
        [SerializeField] public string username;

        [Header("Audio Settings")]
        [Range(0f, 1f)] public float musicVolume = 100f;
        [Range(0f, 1f)] public float sfxVolume = 100f;
        
        [SerializeField] private AudioMixer audioMixer;

        private void Awake() {
            if (Singleton != null && Singleton != this) {
                DestroyImmediate(this.gameObject);
                return;
            }

            Singleton = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
        }

        public void SetMusicVolume(float percentValue) {
            musicVolume = Mathf.Clamp01(percentValue / 100f);
            audioMixer.SetFloat("MusicVolume", percentValue - 80);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float percentValue) {
            sfxVolume = Mathf.Clamp01(percentValue / 100f);
            audioMixer.SetFloat("SFXVolume", percentValue - 80);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        public float GetMusicVolumePercent() {
            return musicVolume * 100f;
        }

        public float GetSfxVolumePercent() {
            return sfxVolume * 100f;
        }

        public void LoadPreferences() {
            username = PlayerPrefs.GetString("Username", "Player");
            audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 100f) - 80);
            audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 100f) - 80);
        }

        public void SaveUserSettings() {
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.Save();
        }
    }
}