using System;
using Treep.Player;

namespace Treep.Weapon {
    public class WeaponHitBox {
        public IShapesHitbox Top;
        public IShapesHitbox Bottom;
        public IShapesHitbox Left;
        public IShapesHitbox Right;

        private Looking _currentlooking;

        public IShapesHitbox Current {
            get {
                return this._currentlooking switch {
                    Looking.Top => this.Top,
                    Looking.Bottom => this.Bottom,
                    Looking.Left => this.Left,
                    Looking.Right => this.Right,
                    _ => throw new Exception("Unknown Looking in current looking hitbox")
                };
            }
        }

        public void UpdateLooking(Looking looking) {
            this._currentlooking = looking;
        }

        public WeaponHitBox(IShapesHitbox top, IShapesHitbox bottom, IShapesHitbox left, IShapesHitbox right) {
            this.Top = top;
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
        }
    }
}
