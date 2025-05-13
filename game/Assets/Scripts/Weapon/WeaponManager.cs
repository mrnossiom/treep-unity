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
        private Dictionary<Weapons, ICloseWeapon> _weapons;

        private Weapons _currentWeapon = Weapons.Stick;
        private ICloseWeapon Weapon => this._weapons[this._currentWeapon];

        public int Damage => this.Weapon.Damage;
        public IShapesHitbox HitBox => this.Weapon.HitBox.Current;

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

        public void UpdateLooking(Looking currentLooking) {
            this.Weapon.HitBox?.UpdateLooking(currentLooking);
        }

        public void OnDrawGizmosSelected() {
            if (this.Weapon.HitBox == null) return;
            Gizmos.color = Color.red;
            this.Weapon.HitBox.Current.DrawGizmo(this.transform.position);
        }

        public override string ToString() {
            return "Current Weapon : " + this._currentWeapon;
        }
    }
}
