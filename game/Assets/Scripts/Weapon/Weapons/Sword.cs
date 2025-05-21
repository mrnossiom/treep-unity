using UnityEngine;

namespace Treep.Weapon {
    public class Sword : CircleHitboxCloseWeapon {
        private void Awake() {
            this.Name = "Sword";
            this.Damage = 4;
            this.AttackRate = 3f;

            this.BaseAwake(2f);
        }

        public override string ToString() {
            return $"{this.Name} damage: {this.Damage} attack rate: {this.AttackRate}";
        }
    }
}
