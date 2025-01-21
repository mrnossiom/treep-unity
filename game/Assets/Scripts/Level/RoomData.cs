using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treep.Level {
    [Serializable]
    public enum DoorSize {
        Vertical6,
    }

    internal static class DoorSizeMethods {
        public static Vector2 GetDoorSizeVector(this DoorSize size) {
            return size switch {
                DoorSize.Vertical6 => new Vector2(1, 6),
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }

    [Serializable]
    public class DoorData {
        public Vector2 position = Vector2.one;
        public DoorSize size = DoorSize.Vertical6;

        public Vector2[] AdjacentDeltas() {
            var delta = size.GetDoorSizeVector();
            switch (size) {
                case DoorSize.Vertical6:
                    delta.y = 0;
                    return new[] { delta, -delta };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class RoomData : MonoBehaviour {
        public Vector2 size = Vector2.one;

        public List<DoorData> doors = new();

        public List<Vector2> enemySpawners = new();
        public List<Vector2> spawnPoints = new();

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + (Vector3)size / 2, size);

            // draw doors positions
            Gizmos.color = Color.red;
            foreach (var door in doors) {
                var doorSizeVector = door.size.GetDoorSizeVector();
                Gizmos.DrawWireCube(
                    transform.position + (Vector3)door.position + (Vector3)(doorSizeVector / 2), doorSizeVector);
            }

            Gizmos.color = Color.magenta;
            foreach (var enemySpawner in enemySpawners) {
                Gizmos.DrawWireSphere(transform.position + (Vector3)enemySpawner, .5f);
            }

            Gizmos.color = Color.green;
            foreach (var spawnPoint in spawnPoints) {
                Gizmos.DrawWireSphere(transform.position + (Vector3)spawnPoint, .5f);
            }
        }
    }
}
