using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treep
{
    public class LoadPlayground : MonoBehaviour
    {
        private void Awake() {
            if (SceneManager.sceneCount == 1) {
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);
            }
        }
    }
}
