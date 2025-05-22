using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treep.Level {
    public class LevelAssembler : MonoBehaviour {
        [SerializeField] private RoomProvider roomProvider;
        [SerializeField] private GameObject enemyPrefab;

        // TODO: non-linear level blueprint and evolver
        [SerializeField] private List<RoomKind> levelBlueprint;

        public List<RoomKind> LevelBlueprint => this.levelBlueprint;
        public RoomProvider RoomProvider => this.roomProvider;

        public Vector2? GenerateLevel(int seed) {
            var roomsBook = this.roomProvider.CollectRooms();

            var rng = new System.Random(seed);

            var evolver = new LevelEvolver(this.levelBlueprint, roomsBook);
            // level is not solvable
            if (!evolver.EvolveRoot(rng)) return null;

            this.PlaceLevelRooms(evolver.PlacedRooms);

            if (evolver.SpawnPoints.Count == 0) {
                Debug.LogError("No spawn point");
                return null;
            }

            foreach (var enemySpawnerPos in evolver.EnemySpawners) {
                // + Vector2.up because enemy spawn in the ground
                Object.Instantiate(this.enemyPrefab, enemySpawnerPos + new Vector2(0, 2), Quaternion.identity);
            }


            return evolver.SpawnPoints.First();
        }

        public void ClearChildren() {
            while (this.transform.childCount > 0) {
                Object.DestroyImmediate(this.transform.GetChild(0).gameObject);
            }
        }

        private void PlaceLevelRooms(IEnumerable<PlacedRoom> rooms) {
            this.ClearChildren();
            foreach (var room in rooms) {
                Object.Instantiate(room.Template, room.Position, Quaternion.identity, this.transform);
            }
        }
    }
}
