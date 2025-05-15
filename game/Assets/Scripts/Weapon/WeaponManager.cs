using System.Collections.Generic;
using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public enum Weapons {
        Stick,
        Spear
    }

    public class WeaponManager : MonoBehaviour {
        public Stick stick;
        public Spear spear;
        private Dictionary<Weapons, ICloseWeapon> _weapons = new();

        private Weapons _currentWeapon = Weapons.Stick;

        private ICloseWeapon Weapon => this._weapons.Count == 0 ? null : this._weapons[this._currentWeapon];


        public int Damage => this.Weapon.Damage;
        public IShapesHitbox HitBox => this.Weapon.Hitbox.Current;

        public float AttackRate => this.Weapon.AttackRate;


        private void Awake() {
            this._weapons = new Dictionary<Weapons, ICloseWeapon>();
            this._weapons.Add(Weapons.Stick, this.stick);
            this._weapons.Add(Weapons.Spear, this.spear);

            this._currentWeapon = Weapons.Spear;
        }

        public bool SwitchWeapon(Weapons newWeapon) {
            this._currentWeapon = newWeapon;
            return true;
        }

        public void UpdateLooking(LookDirection currentLookDirection) {
            this.Weapon.Hitbox?.UpdateLooking(currentLookDirection);
        }

        public void OnDrawGizmosSelected() {
            if (this.Weapon == null) return;
            Gizmos.color = Color.red;
            this.Weapon.Hitbox.Current.DrawGizmo(this.transform.position);
        }

        public override string ToString() {
            return "Current Weapon : " + this._currentWeapon;
        }
    }
}
