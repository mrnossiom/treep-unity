using UnityEngine;

namespace Treep.Weapon {
    public class Spear : RectHitboxCloseWeapon {
        public int Durability { get; set; }

        private void Awake() {
            this.Name = "Spear";
            this.Damage = 1;
            this.Durability = 1;
            this.AttackRate = 2f;

            this.SetAttackHitbox(new Vector2(1, 4), new Vector2(4, 1));
        }
    }
}
