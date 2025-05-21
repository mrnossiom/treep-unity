using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class CircleHitboxCloseWeapon : MonoBehaviour, ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }
        public float AttackRate { get; set; }
        public WeaponHitbox Hitbox { get; set; }
        public SpriteRenderer SpriteRenderer { get; set; }
        public Animator Animator { get; set; }

        private Circle Top;
        private Circle Bottom;
        private Circle Left;
        private Circle Right;


        protected void BaseAwake(float legacyRange) {
            this.Top = new Circle(legacyRange, PlayerCombat.AttackPointTop);
            this.Bottom = new Circle(legacyRange, PlayerCombat.AttackPointBottom);
            this.Left = new Circle(legacyRange, PlayerCombat.AttackPointLeft);
            this.Right = new Circle(legacyRange, PlayerCombat.AttackPointRight);

            this.Hitbox = new WeaponHitbox(this.Top, this.Bottom, this.Left, this.Right);

            this.SpriteRenderer = this.GetComponent<SpriteRenderer>();
            this.Animator = this.GetComponent<Animator>();
        }
    }
}
