using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Treep.Weapon {
    public enum Weapons {
        Stick,
        Spear
    }

    public class WeaponManager : MonoBehaviour {
        private Dictionary<Weapons, ICloseWeapon> _weapons;

        private Weapons _currentWeapon = Weapons.Stick;
        public ICloseWeapon Weapon => this._weapons[this._currentWeapon];


        private void Awake() {
            var stick = this.gameObject.AddComponent<Stick>();
            var spear = this.gameObject.AddComponent<Spear>();
            this._weapons = new Dictionary<Weapons, ICloseWeapon>();
            this._weapons.Add(Weapons.Stick, stick);
            this._weapons.Add(Weapons.Spear, spear);

            this._currentWeapon = Weapons.Stick;
            //this.CurrentWeapon = this.gameObject.AddComponent<Stick>();
        }

        public bool SwitchWeapon(Weapons newWeapon) {
            this._currentWeapon = newWeapon;
            return true;
        }

        public void OnDrawGizmosSelected() {
            if (this.Weapon.HitBox != null) {
                Utils.Collider2DGizmoUtility.GizmosDraw2DCollider(this.Weapon.HitBox.Current);
            }
            else {
                Debug.Log("Weapon.HitBox est null");
            }
        }

        public override string ToString() {
            return "Current Weapon : " + this._currentWeapon;
        }
    }
}
