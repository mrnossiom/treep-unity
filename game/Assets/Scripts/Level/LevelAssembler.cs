using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Treep.Level {
    public class LevelAssembler : MonoBehaviour {
        [SerializeField] private RoomProvider roomProvider;

        // TODO: non-linear level blueprint and evolver
        [SerializeField] private List<RoomKind> levelBlueprint;

        public List<RoomKind> LevelBlueprint => levelBlueprint;
        public RoomProvider RoomProvider => roomProvider;

        public Vector2? GenerateLevel(int seed) {
            var roomsBook = roomProvider.CollectRooms();

            var rng = new System.Random(seed);

            var evolver = new LevelEvolver(levelBlueprint, roomsBook);
            if (!evolver.EvolveRoot(rng)) return null;

            PlaceLevelRooms(evolver.PlacedRooms);

            if (evolver.SpawnPoints.Count == 0) {
                Debug.LogError("No spawn point");
                return null;
            }

            return evolver.SpawnPoints.First();
        }

        public void ClearChildren() {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        private void PlaceLevelRooms(IEnumerable<PlacedRoom> rooms) {
            ClearChildren();
            foreach (var room in rooms) {
                Instantiate(room.Template, room.Position, Quaternion.identity, transform);
            }
        }
    }
}
