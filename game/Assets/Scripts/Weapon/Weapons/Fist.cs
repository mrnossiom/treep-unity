using UnityEngine;

namespace Treep.Weapon {
    public class Fist : CircleHitboxCloseWeapon {
        private void Awake() {
            this.Name = "fist";
            this.Damage = 1;
            this.AttackRate = 4f;

            this.BaseAwake(1f);
        }

        public override string ToString() {
            return $"{this.Name} damage: {this.Damage} attack rate: {this.AttackRate}";
        }
    }
}
