using System.Collections.Generic;
using UnityEngine;

namespace Treep.Levels {
    [System.Serializable]
    public class DoorData {
        public Vector2 position = Vector2.one;
        public Vector2 size = Vector2.one;
    }

    public class RoomData : MonoBehaviour {
        public Vector2 size = Vector2.one;

        public List<DoorData> doors = new();

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube((Vector3)size / 2, (Vector3)size);

            // draw doors positions
            Gizmos.color = Color.red;
            foreach (var door in doors) {
                Gizmos.DrawWireCube((Vector3)door.position + (Vector3)door.size / 2, (Vector3)door.size);
            }
        }
    }
}
