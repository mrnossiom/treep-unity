using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class RectHitboxCloseWeapon : MonoBehaviour, ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }
        public float AttackRate { get; set; }
        public WeaponHitbox Hitbox { get; set; }


        private Rect Top { get; set; }
        private Rect Bottom { get; set; }
        private Rect Left { get; set; }
        private Rect Right { get; set; }

        protected void SetAttackHitbox(Vector2 topSize, Vector2 rightSize) {
            this.Top = new Rect(topSize,
                PlayerCombat.AttackPointTop + new Vector2(0f, topSize.y / 2));
            this.Bottom = new Rect(new Vector2(topSize.x, -topSize.y),
                PlayerCombat.AttackPointBottom + new Vector2(0f, -topSize.y / 2));
            this.Left = new Rect(new Vector2(-rightSize.x, rightSize.y),
                PlayerCombat.AttackPointLeft + new Vector2(-rightSize.x / 4, 0f));
            this.Right = new Rect(rightSize,
                PlayerCombat.AttackPointRight + new Vector2(rightSize.x / 4, 0f));

            this.Hitbox = new WeaponHitbox(this.Top, this.Bottom, this.Left, this.Right);
        }
    }
}
