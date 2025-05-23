using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public enum Weapons {
        Fist,
        Stick,
        Sword,
        Spear
    }

    public class WeaponManager : MonoBehaviour {
        public Fist fist;
        public Stick stick;
        public Sword sword;
        public Spear spear;

        public Weapons CurrentWeapon { get; private set; }
        public float Damage => this.Weapon.Damage * Player.Player.Singleton.damageMultiplier;
        public IShapesHitbox HitBox => this.Weapon.Hitbox.Current;
        public float AttackRate => this.Weapon.AttackRate;
        
        private ICloseWeapon Weapon => this._weapons.Count == 0 ? null : this._weapons[this.CurrentWeapon];
        private Dictionary<Weapons, ICloseWeapon> _weapons = new();
        
        private void Awake() {
            this._weapons = new Dictionary<Weapons, ICloseWeapon>();
            this._weapons.Add(Weapons.Fist, this.fist);
            this._weapons.Add(Weapons.Sword, this.sword);
            this._weapons.Add(Weapons.Stick, this.stick);
            this._weapons.Add(Weapons.Spear, this.spear);

            this.CurrentWeapon = PlayerController.StartWeapon;
        }

        public bool SwitchWeapon(Weapons newWeapon) {
            this.CurrentWeapon = newWeapon;
            return true;
        }

        public void UpdateLooking(LookDirection currentLookDirection) {
            if (this.Weapon is null) {
                Debug.LogError("Weapon est null");
            }
            else {
                this.Weapon.Hitbox?.UpdateLooking(currentLookDirection);
            }
        }

        public void OnDrawGizmosSelected() {
            if (this.Weapon == null) return;
            Gizmos.color = Color.red;
            this.Weapon.Hitbox.Current.DrawGizmo(this.transform.position);
        }

        public override string ToString() {
            return "Current Weapon: " + this.CurrentWeapon;
        }
    }
}
