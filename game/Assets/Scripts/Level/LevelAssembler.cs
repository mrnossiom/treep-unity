using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace Treep.Level {
    public class LevelAssembler : MonoBehaviour {
        [SerializeField] private LevelBlueprint levelBlueprint;
        [SerializeField] private RoomProvider roomProvider;

        [SerializeField] private int seed;

        public bool GenerateLevel() {
            var blueprint = levelBlueprint.CollectBlueprint();
            var roomsBook = roomProvider.CollectRooms();

            var rng = new Random(seed);

            var evolver = new LevelEvolver(blueprint, roomsBook);
            var level = evolver.EvolveRoot(rng);
            if (level is null) return false;

            PlaceLevelRooms(level);

            return true;
        }

        public void Clear() {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        private void PlaceLevelRooms(List<PlacedRoom> rooms) {
            Clear();
            foreach (var room in rooms) {
                Instantiate(room.Template, room.Position, Quaternion.identity, transform);
            }
        }
    }

    [CustomEditor(typeof(LevelAssembler))]
    class LevelAssemblerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var levelAssembler = (LevelAssembler)target;

            GUILayout.Space(15);

            if (GUILayout.Button("Generate Level")) {
                if (levelAssembler.GenerateLevel()) {
                    Debug.Log("done");
                }
                else {
                    Debug.Log("level is not solvable");
                }
            }

            if (GUILayout.Button("Clear")) {
                levelAssembler.Clear();
            }
        }
    }
}
