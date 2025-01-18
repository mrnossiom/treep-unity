using System.Collections.Generic;
using UnityEngine;

namespace Treep.Level {
    [System.Serializable]
    public enum RoomKind {
        Spawn,

        Normal,
        Shop,

        Exit,
    }


    [CreateAssetMenu(fileName = "RoomKindProvider", menuName = "Scriptable Objects/Room Kind Provider")]
    public class RoomKindProvider : ScriptableObject {
        public RoomKind kind;

        public List<RoomData> rooms;
    }
}