using UnityEngine;
using Treep.State;

namespace Treep.Interface {
    public class HudController : MonoBehaviour {
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private LobbyMenu lobbyMenu;
        [SerializeField] private HealthBar healthBar;
        
        private GameStateManager gameStateManager;
        
        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
            }
            
            gameStateManager = FindAnyObjectByType<GameStateManager>();
            if (gameStateManager is not null && gameStateManager.StateKind == GameStateKind.Level) {
                lobbyMenu.gameObject.SetActive(false);
                healthBar.gameObject.SetActive(true);
                this.lobbyMenu.UpdateReadyButtonText(false);
            }
            
            if (gameStateManager is not null && gameStateManager.StateKind == GameStateKind.Lobby) {
                lobbyMenu.gameObject.SetActive(true);
                healthBar.gameObject.SetActive(false);
            }
        }
    }
}
