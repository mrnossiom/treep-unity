using UnityEngine;

namespace Treep.Weapon {
    public interface IShapesHitbox {
        public Vector2 LocalPos { get; set; }
        public Vector2 GetPos(Vector2 gameObjectPos);

        public void DrawGizmo(Vector2 pos);
    }
}
