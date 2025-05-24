using Mirror;
using Treep.SFX;
using Treep.State;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Treep.Interface
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button createGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private NetworkController networkControllerPrefab;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsMenu settingsPanel;
        [SerializeField] private JoinGameMenu joinGameMenu;
        [SerializeField] private Button quitButton;
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioMixer audioMixer;
        
        void Start()
        {
            createGameButton.onClick.AddListener(() =>
            {
                DontDestroyOnLoad(Instantiate(networkControllerPrefab));
                NetworkManager.singleton.StartHost();
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });
            joinGameButton.onClick.AddListener(() =>
            {
                DontDestroyOnLoad(Instantiate(networkControllerPrefab));
                joinGameMenu.gameObject.SetActive(true);
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });

            settingsButton.onClick.AddListener(() => {
                this.settingsPanel.gameObject.SetActive(true);
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
            });
            
            this.quitButton.onClick.AddListener(() => {
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
                Application.Quit();
            });
        }
    }
}
