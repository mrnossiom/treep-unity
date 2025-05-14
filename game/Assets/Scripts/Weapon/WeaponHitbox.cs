using System;
using Treep.Player;

namespace Treep.Weapon {
    public class WeaponHitbox {
        private readonly IShapesHitbox Top;
        private readonly IShapesHitbox Bottom;
        private readonly IShapesHitbox Left;
        private readonly IShapesHitbox Right;

        private LookDirection _currentLookDirection;

        public IShapesHitbox Current {
            get {
                return this._currentLookDirection switch {
                    LookDirection.Top => this.Top,
                    LookDirection.Bottom => this.Bottom,
                    LookDirection.Left => this.Left,
                    LookDirection.Right => this.Right,
                    _ => throw new Exception("Unknown Looking in current looking hitbox")
                };
            }
        }

        public void UpdateLooking(LookDirection lookDirection) {
            this._currentLookDirection = lookDirection;
        }

        public WeaponHitbox(IShapesHitbox top, IShapesHitbox bottom, IShapesHitbox left, IShapesHitbox right) {
            this.Top = top;
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
        }
    }
}
