using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface {
    public class LobbyMenu : MonoBehaviour {
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI readyButtonText;
        
        private bool isReady;

        public void OnReadyButtonClicked() {
            if (NetworkClient.localPlayer != null) {
                var player = NetworkClient.localPlayer.GetComponent<Treep.Player.Player>();
                
                if (player != null) {
                    isReady = !isReady;
                    player.CmdSetReady(isReady);
                    UpdateReadyButtonText(isReady);
                }
            }
        }

        public void UpdateReadyButtonText(bool ready) {
            if (readyButtonText != null) readyButtonText.text = ready ? "Cancel" : "Ready";
        }
    }
}
