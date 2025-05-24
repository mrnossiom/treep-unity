using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Treep.Level {
    public class LevelAssembler : MonoBehaviour {
        [SerializeField] private RoomProvider roomProvider;
        [SerializeField] private GameObject enemyPrefab;

        public Bounds bounds;

        // TODO: non-linear level blueprint and evolver
        [SerializeField] private List<RoomKind> levelBlueprint;

        [SerializeField] private Transform levelOuterMask;

        public List<RoomKind> LevelBlueprint => this.levelBlueprint;
        public RoomProvider RoomProvider => this.roomProvider;

        public Vector2? GenerateLevel(int seed) {
            var roomsBook = this.roomProvider.CollectRooms();

            var rng = new System.Random(seed);

            var evolver = new LevelEvolver(this.levelBlueprint, roomsBook);
            // level is not solvable
            if (!evolver.EvolveRoot(rng)) return null;
            this.bounds = evolver.Bounds;

            if (evolver.SpawnPoints.Count == 0) {
                Debug.LogError("No spawn point");
                return null;
            }

            this.PlaceLevelRooms(evolver.PlacedRooms);

            // place enemies
            var enemyContainer = new GameObject("Enemies");
            enemyContainer.transform.parent = this.transform;
            foreach (var enemySpawnerPos in evolver.EnemySpawners) {
                // avoid spawning the enemy in the ground
                var spawnerPos = enemySpawnerPos + Vector2.up * 2;
                Object.Instantiate(this.enemyPrefab, spawnerPos, Quaternion.identity, enemyContainer.transform);
            }

            // place end doors
            foreach (var exitDoorPos in evolver.ExitPoints) {
                // TODO: make door that ticks GameStateManager
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
                var roomInstance
                    = Object.Instantiate(room.Template, room.Position, Quaternion.identity, this.transform);
                var outerMask = Object.Instantiate(this.levelOuterMask, roomInstance.transform);
                outerMask.position = room.Area.center;
                outerMask.localScale = room.Area.size;
            }
        }

        private void OnDrawGizmos() {
            Gizmos.DrawWireCube(this.bounds.center, this.bounds.center);
        }
    }
}
