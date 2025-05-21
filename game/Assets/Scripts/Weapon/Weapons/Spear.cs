using UnityEngine;

namespace Treep.Weapon {
    public class Spear : RectHitboxCloseWeapon {
        private void Awake() {
            this.Name = "Spear";
            this.Damage = 3;
            this.AttackRate = 2f;

            this.SetAttackHitbox(new Vector2(1, 4), new Vector2(4, 1));
        }

        public override string ToString() {
            return $"{this.Name} damage: {this.Damage} attack rate: {this.AttackRate}";
        }
    }
}
