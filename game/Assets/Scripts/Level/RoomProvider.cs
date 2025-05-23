using System.Collections.Generic;
using System.Linq;
using Treep.Utils;
using UnityEngine;

namespace Treep.Level {
    [CreateAssetMenu(fileName = "RoomProvider", menuName = "Scriptable Objects/Room Provider")]
    public class RoomProvider : ScriptableObject {
        [SerializeField] private PseudoDictionary<RoomKind, RoomKindProvider> roomProviders = new();

        public Dictionary<RoomKind, List<RoomData>> CollectRooms() {
            var providers = this.roomProviders.ToActualDictionary();

            return providers.ToDictionary(provider => provider.Key, provider => provider.Value?.rooms);
        }
    }
}
