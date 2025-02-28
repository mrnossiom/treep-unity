using UnityEngine;
using Treep.State;

namespace Treep.Interface {
    public class HudController : MonoBehaviour {
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private LobbyMenu lobbyMenu;
        
        private GameStateManager gameStateManager;
        
        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
            }
            
            gameStateManager = FindAnyObjectByType<GameStateManager>();
            if (gameStateManager is not null && gameStateManager.stateKind == GameStateKind.Level) {
                lobbyMenu.gameObject.SetActive(false);
            }
        }
    }
}
