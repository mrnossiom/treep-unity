using Treep.Utils;
using UnityEngine;

namespace Treep.Levels {
    [CreateAssetMenu(fileName = "RoomProvider", menuName = "Scriptable Objects/Room Provider")]
    public class RoomProvider : ScriptableObject {
        public PseudoDictionary<RoomKind, RoomKindProvider> roomProviders = new();
    }
}
