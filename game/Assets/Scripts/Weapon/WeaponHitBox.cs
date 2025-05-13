using System;
using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class WeaponHitBox {
        public Collider2D Top;
        public Collider2D Bottom;
        public Collider2D Left;
        public Collider2D Right;

        private Looking _currentlooking;

        public Collider2D Current {
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

        public WeaponHitBox(Collider2D top, Collider2D bottom, Collider2D left, Collider2D right) {
            this.Top = top;
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
        }
    }
}
