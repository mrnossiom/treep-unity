using UnityEngine;

namespace Treep.Weapon {
    public class Stick : NoHitboxCloseWeapon {
        private void Awake() {
            this.Name = "Stick";
            this.Damage = 1;
            this.AttackRate = 4f;

            this.BaseAwake(2f);
        }


        public override string ToString() {
            return $"Name : {this.Name}\n" +
                   $"Damage : {this.Damage}\n" +
                   $"AttackRate : {this.AttackRate}\n";
        }
    }
}
