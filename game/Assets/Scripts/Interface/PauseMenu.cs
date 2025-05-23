using Mirror;
using Treep.SFX;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Treep.Interface {
    public class PauseMenu : MonoBehaviour {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button quitRoomButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsMenu settingsPane;
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioMixer audioMixer;

        private void Awake() {
            closeButton.onClick.AddListener(() => {
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
                gameObject.SetActive(false);
            });
            
            settingsButton.onClick.AddListener(() => {
                settingsPane.gameObject.SetActive(true); 
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });
            
            quitRoomButton.onClick.AddListener(() => {
                NetworkManager.singleton.StopHost();
                NetworkManager.singleton.StopClient();
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });
        }
    }
}
