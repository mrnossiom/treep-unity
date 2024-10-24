using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treep.Network {
    public class NetworkManagerUI : MonoBehaviour {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;

        private void Start() {
            hostButton.onClick.AddListener(() => {
                Debug.Log("Hosting started...");
                NetworkManager.Singleton.StartHost();
                SceneManager.LoadSceneAsync("Playground", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Menu");
            });
            clientButton.onClick.AddListener(() => {
                Debug.Log("Client started...");
                NetworkManager.Singleton.StartClient();
                SceneManager.LoadSceneAsync("Playground", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Menu");
            });
            serverButton.onClick.AddListener(() => {
                Debug.Log("Server started...");
                NetworkManager.Singleton.StartServer();
                SceneManager.LoadScene("Playground", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Menu");
            });
        }
    }
}
