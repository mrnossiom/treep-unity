using UnityEngine;

namespace Treep.State {
    public class Settings : MonoBehaviour {
        public static Settings Singleton;

        [Header("User Settings")]
        [SerializeField] public string username;

        [Header("Audio Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.75f;
        [Range(0f, 1f)] public float sfxVolume = 0.75f;

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
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float percentValue) {
            sfxVolume = Mathf.Clamp01(percentValue / 100f);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        public float GetMusicVolumePercent() {
            return musicVolume * 100f;
        }

        public float GetSFXVolumePercent() {
            return sfxVolume * 100f;
        }

        public void LoadPreferences() {
            username = PlayerPrefs.GetString("Username", "Player");
            musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume", 0.75f));
            sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("SFXVolume", 0.75f));
        }

        public void SaveUserSettings() {
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.Save();
        }
    }
}