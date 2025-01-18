using System.Collections.Generic;
using UnityEngine;

namespace Treep.Level {
    [CreateAssetMenu(fileName = "LevelBlueprint", menuName = "Scriptable Objects/Level Blueprint")]
    public class LevelBlueprint : ScriptableObject {
        [SerializeField] private List<RoomKind> blueprint;

        // TODO: add graph/tree description

        public List<RoomKind> CollectBlueprint() {
            return blueprint;
        }
    }
}
