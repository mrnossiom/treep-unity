using System;
using Mirror;
using Treep.Level;
using UnityEngine;

namespace Treep.State.GameStates
{
    [Serializable]
    public class GameStateLevel : IGameState
    {
        public System.Random Rng;

        private LevelAssembler _container;

        public GameStateLevel(int seed)
        {
            Rng = new System.Random(seed);
        }

        public void OnEnter(GameStateManager manager)
        {
            var target = UnityEngine.Object.Instantiate(manager.UglyLevelAccessorToRemoveLater);
            var spawnPoint = target.GenerateLevel(Rng.Next());
            if (spawnPoint is null) throw new NotImplementedException();

            foreach (var (id, conn) in NetworkServer.connections)
            {
                conn.identity.transform.position = (Vector3)spawnPoint;
                conn.identity.transform.GetComponent<Player.Player>().SetSimultated(true);
            }

            _container = target;
        }

        public void OnExit()
        {
            UnityEngine.Object.Destroy(_container.gameObject);
            _container = null;
        }
    }
}
