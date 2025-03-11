using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface {
    public class PauseMenu : MonoBehaviour {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button quitRoomButton;

        private void Awake() {
            closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });

            quitRoomButton.onClick.AddListener(() => {
                NetworkManager.singleton.StopHost();
                NetworkManager.singleton.StopClient();
            });
        }
    }
}
