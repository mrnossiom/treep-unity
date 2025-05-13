using UnityEngine;

namespace Treep.Weapon {
    public class Circle : IShapesHitbox {
        public Vector2 LocalPos { get; set; }
        public float Radius;

        public Circle(float radius, Vector2 pos = default) {
            this.LocalPos = pos;
            this.Radius = radius;
        }

        public Vector2 GetPos(Vector2 gameObjectPos) {
            return this.LocalPos + gameObjectPos;
        }

        public void DrawGizmo(Vector2 pos) {
            Gizmos.DrawWireSphere(this.GetPos(pos), this.Radius);
        }
    }
}
