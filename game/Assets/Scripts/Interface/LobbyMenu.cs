using System;
using Mirror;
using TMPro;
using Treep.SFX;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Treep.Interface {
    public class LobbyMenu : MonoBehaviour {
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI readyButtonText;
        
        private bool isReady;
        
        [SerializeField] private AudioClip buttonPress;
        [SerializeField] private AudioMixer audioMixer;

        public void OnReadyButtonClicked() {
            if (NetworkClient.localPlayer != null) {
                var player = NetworkClient.localPlayer.GetComponent<Treep.Player.Player>();
                
                if (player != null) {
                    isReady = !isReady;
                    player.CmdSetReady(isReady);
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
                    UpdateReadyButtonText(isReady);
                }
            }
        }

        public void UpdateReadyButtonText(bool ready) {
            if (readyButtonText != null) readyButtonText.text = ready ? "Cancel" : "Ready";
        }
    }
}
