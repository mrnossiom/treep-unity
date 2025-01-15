using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treep.Utils {
    public class LoadPlayground : MonoBehaviour {
        private void Awake() {
            if (SceneManager.sceneCount == 1) {
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);
            }
        }
    }
}