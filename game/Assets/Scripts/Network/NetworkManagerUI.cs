using Mirror;
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
                NetworkManager.singleton.StartHost();
                SceneManager.LoadSceneAsync("Playground", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Menu");
            });
            clientButton.onClick.AddListener(() => {
                NetworkManager.singleton.StartClient();
                SceneManager.LoadSceneAsync("Playground", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Menu");
            });
            serverButton.onClick.AddListener(() => {
                NetworkManager.singleton.StartServer();
                SceneManager.LoadScene("Playground", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Menu");
            });
        }
    }
}
