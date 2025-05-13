using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class CirleHitboxCloseWeapon : MonoBehaviour, ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }
        public float AttackRate { get; set; }
        public WeaponHitBox HitBox { get; set; }
        protected Circle Top { get; set; }
        protected Circle Bottom { get; set; }
        protected Circle Left { get; set; }
        protected Circle Right { get; set; }


        protected void BaseAwake(float legacyRange) {
            this.Top = new Circle(legacyRange);
            this.Bottom = new Circle(legacyRange);
            this.Left = new Circle(legacyRange);
            this.Right = new Circle(legacyRange);


            this.Top.LocalPos = PlayerCombat.AttackPointTop;
            this.Bottom.LocalPos = PlayerCombat.AttackPointBottom;
            this.Left.LocalPos = PlayerCombat.AttackPointLeft;
            this.Right.LocalPos = PlayerCombat.AttackPointRight;

            this.HitBox = new WeaponHitBox(this.Top, this.Bottom, this.Left, this.Right);
        }


        public override string ToString() {
            return $"Name : {this.Name}\n" +
                   $"Damage : {this.Damage}\n" +
                   $"AttackRate : {this.AttackRate}\n";
        }
    }
}
