using System;
using Mirror;
using Treep.Level;
using UnityEngine;

namespace Treep.State.GameStates {
    public class GameStateLobby : IGameState {
        public System.Random Rng;

        private LevelAssembler _container;
        
        public GameStateLobby() {
            this.Rng = new System.Random(0);
        }
        
        public void OnEnter(GameStateManager manager) {
            var target = UnityEngine.Object.Instantiate(manager.UglyLobbyAccessorToRemoveLater);
            var spawnPoint = target.GenerateLevel(this.Rng.Next());
            if (spawnPoint is null) throw new NotImplementedException();
            
            var player = NetworkClient.connection.identity.transform;
            player.position = (Vector3)spawnPoint;
            player.GetComponent<Player.Player>().SetSimulated(true);

            this._container = target;
        }

        public void OnExit() {
            UnityEngine.Object.Destroy(this._container.gameObject);
            this._container = null;
        }
    }
}
