using Mirror;
using TMPro;
using Treep.SFX;
using Treep.State;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Treep.Interface {
    public class SettingsMenu : MonoBehaviour {
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private Slider mainSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Button startServerButton;
        [SerializeField] private NetworkController networkControllerPrefab;

        private void Start() {
            closeButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });
            
            usernameInput.text = Settings.Singleton.username;
            usernameInput.onEndEdit.AddListener(username => {
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
                Settings.Singleton.username = username;
                Settings.Singleton.SaveUserSettings();
            });
            
            musicSlider.value = Settings.Singleton.GetMusicVolumePercent();
            sfxSlider.value = Settings.Singleton.GetSfxVolumePercent();
            this.mainSlider.value = Settings.Singleton.GetMasterVolumePercent();

            musicSlider.onValueChanged.AddListener(Settings.Singleton.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(Settings.Singleton.SetSfxVolume);
            this.mainSlider.onValueChanged.AddListener(Settings.Singleton.SetMasterVolume);
            
            startServerButton.onClick.AddListener(() =>
            {
                DontDestroyOnLoad(Instantiate(networkControllerPrefab));
                NetworkManager.singleton.StartServer();
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });
        }
    }
}