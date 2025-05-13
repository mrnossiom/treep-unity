using UnityEngine;

namespace Treep.Weapon {
    public class Rect : IShapesHitbox {
        public float x;
        public float y;
        public float width;
        public float height;


        public Vector2 LocalPos {
            get => new(this.x, this.y);
            set {
                this.x = value.x;
                this.y = value.y;
            }
        }

        public Vector2 Size {
            get => new(this.width, this.height);
            set {
                this.width = value.x;
                this.height = value.y;
            }
        }

        public Rect(int width, int height, int x = 0, int y = 0) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2 GetPos(Vector2 gameObjectPos) {
            return this.LocalPos + gameObjectPos;
        }

        public void DrawGizmo(Vector2 gameObjectpos) {
            Gizmos.DrawWireCube(this.GetPos(gameObjectpos), this.Size);
        }
    }
}
