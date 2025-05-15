using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface {
    public class PauseMenu : MonoBehaviour {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button quitRoomButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsMenu settingsPane;

        private void Awake() {
            closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });
            
            settingsButton.onClick.AddListener(() => { settingsPane.gameObject.SetActive(true); });
            
            quitRoomButton.onClick.AddListener(() => {
                NetworkManager.singleton.StopHost();
                NetworkManager.singleton.StopClient();
            });
        }
    }
}
