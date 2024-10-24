using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treep.Network {
    public class InGameNetworkManagerUI : MonoBehaviour {
        [SerializeField] private Button quitButton;
        [SerializeField] private Scene menuScene;

        private void Start() {
            quitButton.onClick.AddListener(() => {
                Debug.Log("Quitting session and going back to menu...");
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync("Playground");
            });
        }
    }
}
