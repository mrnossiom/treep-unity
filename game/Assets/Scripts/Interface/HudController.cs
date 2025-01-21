using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface {
    public class HudController : MonoBehaviour {
        [SerializeField] private PauseMenu pauseMenu;


        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
            }
        }
    }
}
