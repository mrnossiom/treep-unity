using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

namespace Treep.Interface {
    public class HudController : MonoBehaviour {
        [SerializeField] private PauseMenu pauseMenu;
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

        private void UpdateReadyButtonText(bool ready) {
            if (readyButtonText != null) readyButtonText.text = ready ? "Annuler" : "Prêt";
        }
        
        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
            }
        }
    }
}
