using System;
using System.Collections.Generic;
using UnityEngine;


namespace Treep.Level {
    [Serializable]
    public enum DoorSize {
        Vertical5,
    }

    static class DoorSizeMethods {
        public static Vector2 GetDoorSizeVector(this DoorSize size) {
            return size switch {
                DoorSize.Vertical5 => new Vector2(1, 5),
                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
            };
        }
    }


    [Serializable]
    public class DoorData {
        public Vector2 position = Vector2.one;
        public DoorSize size = DoorSize.Vertical5;
    }

    public class RoomData : MonoBehaviour {
        public Vector2 size = Vector2.one;

        public List<DoorData> doors = new();

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
        }
    }
}