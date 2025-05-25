using System;
using JetBrains.Annotations;
using Mirror;
using Treep.Level;
using UnityEngine;

namespace Treep.State.GameStates {
    [Serializable]
    public class GameStateLevel : IGameState {
        public System.Random Rng;
        private int _level;

        [CanBeNull] private LevelAssembler _container;

        public GameStateLevel(int seed, int level) {
            this.Rng = new System.Random(seed);
            this._level = level;
        }

        public void OnEnter(GameStateManager manager) {
            var levelPrefab = this._level switch {
                1 => manager.UglyLevel1AccessorToRemoveLater,
                2 => manager.UglyLevel2AccessorToRemoveLater,
                _ => null
            };

            if (levelPrefab is null) {
                manager.TriggerState(GameStateKind.Lobby);
                return;
            }

            var target = UnityEngine.Object.Instantiate(levelPrefab);
            target.exitCallback = () => {
                manager._level += 1;
                manager.ChangeState(new GameStateLevel(manager._seed, manager._level));
            };
            var spawnPoint = target.GenerateLevel(this.Rng.Next());
            if (spawnPoint is null) throw new NotImplementedException();

            var player = NetworkClient.connection.identity.transform;
            player.position = (Vector3)spawnPoint;
            player.GetComponent<Player.Player>().SetSimulated(true);

            this._container = target;

            this.SetupAstarPath(target.bounds);
        }

        private void SetupAstarPath(Bounds bounds) {
            var astarPath = UnityEngine.Object.FindAnyObjectByType<AstarPath>();

            astarPath.data.gridGraph.center = bounds.center;
            astarPath.data.gridGraph.SetDimensions((int)bounds.size.x, (int)bounds.size.y, 1f);

            astarPath.Scan();
        }


        public void OnExit() {
            UnityEngine.Object.Destroy(this._container?.gameObject);
            this._container = null;
        }
    }
}
