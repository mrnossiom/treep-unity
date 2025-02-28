using System;
using Mirror;
using Treep.Level;
using UnityEngine;

namespace Treep.State.GameStates {
    [Serializable]
    public class GameStateLevel : IGameState {
        public System.Random Rng;

        private LevelAssembler _container;

        public GameStateLevel(int seed) {
            this.Rng = new System.Random(seed);
        }

        public void OnEnter(GameStateManager manager) {
            var target = UnityEngine.Object.Instantiate(manager.UglyLevelAccessorToRemoveLater);
            var spawnPoint = target.GenerateLevel(this.Rng.Next());
            if (spawnPoint is null) throw new NotImplementedException();

            var player = NetworkClient.connection.identity.transform;
            player.position = (Vector3)spawnPoint;
            player.GetComponent<Player.Player>().SetSimultated(true);

            this._container = target;
        }

        public void OnExit() {
            UnityEngine.Object.Destroy(this._container.gameObject);
            this._container = null;
        }
    }
}
