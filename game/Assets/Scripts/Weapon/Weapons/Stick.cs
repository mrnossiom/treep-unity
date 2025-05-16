using UnityEngine;

namespace Treep.Weapon {
    public class Stick : CircleHitboxCloseWeapon {
        private void Awake() {
            this.Name = "Stick";
            this.Damage = 2;
            this.AttackRate = 4f;

            this.BaseAwake(2f);
        }

        public override string ToString() {
            return $"{this.Name} damage: {this.Damage} attack rate: {this.AttackRate}";
        }
    }
}
