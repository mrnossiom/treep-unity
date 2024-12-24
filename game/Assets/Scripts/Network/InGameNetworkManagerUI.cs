using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treep.Network {
    public class InGameNetworkManagerUI : MonoBehaviour {
        [SerializeField] private Button quitButton;
        [SerializeField] private Scene menuScene;

        private void Start() {
            quitButton.onClick.AddListener(() => {
                NetworkManager.singleton.StopServer();
                NetworkManager.singleton.StopHost();
                NetworkManager.singleton.StopClient();
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Playground");
            });
        }
    }
}
