using UnityEngine;

namespace Treep.State {
    public class Settings : MonoBehaviour {
        public static Settings Singleton;

        [Header("User Settings")]
        [SerializeField] public string username;

        [Header("Audio Settings")]
        [Range(0.0001f, 1f)] public float musicVolume = 0.75f;
        [Range(0.0001f, 1f)] public float sfxVolume = 0.75f;

        private void Awake() {
            if (Singleton != null && Singleton != this) {
                DestroyImmediate(this.gameObject);
                return;
            }

            Singleton = this;
            DontDestroyOnLoad(gameObject);

            LoadPreferences();
        }

        public void SetMusicVolume(float value) {
            musicVolume = Mathf.Clamp(value, 0.0001f, 1f);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        public void SetSFXVolume(float value) {
            sfxVolume = Mathf.Clamp(value, 0.0001f, 1f);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        public void LoadPreferences() {
            username = PlayerPrefs.GetString("Username", "Player");
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        }

        public void SaveUserSettings() {
            PlayerPrefs.SetString("Username", username);
        }

        public void ResetPreferences() {
            PlayerPrefs.DeleteAll();
            LoadPreferences();
        }
    }
}
