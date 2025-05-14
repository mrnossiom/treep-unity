using UnityEngine;

namespace Treep.Weapon {
    public interface IShapesHitbox {
        public Vector2 LocalPosition { get; set; }

        public Vector2 GetGlobalPosition(Vector2 gameObjectPos) {
            return this.LocalPosition + gameObjectPos;
        }

        public void DrawGizmo(Vector2 pos);
    }

    public class Circle : IShapesHitbox {
        public Vector2 LocalPosition { get; set; }
        public float Radius { get; }

        public Circle(float radius, Vector2 localPosition) {
            this.LocalPosition = localPosition;
            this.Radius = radius;
        }

        public void DrawGizmo(Vector2 pos) {
            Gizmos.DrawWireSphere(((IShapesHitbox)this).GetGlobalPosition(pos), this.Radius);
        }
    }

    public class Rect : IShapesHitbox {
        public Vector2 LocalPosition { get; set; }
        public Vector2 Size { get; set; }


        public Rect(Vector2 size, Vector2 position) {
            this.LocalPosition = position;
            this.Size = size;
        }

        public void DrawGizmo(Vector2 gameObjectpos) {
            Gizmos.DrawWireCube(((IShapesHitbox)this).GetGlobalPosition(gameObjectpos), this.Size);
        }
    }
}
