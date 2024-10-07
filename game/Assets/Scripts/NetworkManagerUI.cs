using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KrommProject {
    public class NetworkManagerUI : MonoBehaviour {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;

        private void Start() {
            hostButton.onClick.AddListener(() => {
                Debug.Log("Hosting started...");
                NetworkManager.Singleton.StartHost();
            });
            clientButton.onClick.AddListener(() => {
                Debug.Log("Client started...");
                NetworkManager.Singleton.StartClient();
            });
            serverButton.onClick.AddListener(() => {
                Debug.Log("Server started...");
                NetworkManager.Singleton.StartServer();
            });
        }
    }
}
