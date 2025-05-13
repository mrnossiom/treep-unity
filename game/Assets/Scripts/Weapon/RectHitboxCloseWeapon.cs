using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class RectHitboxCloseWeapon : MonoBehaviour, ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }
        public float AttackRate { get; set; }
        public WeaponHitBox HitBox { get; set; }


        public Rect Top { get; set; }
        public Rect Bottom { get; set; }
        public Rect Left { get; set; }
        public Rect Right { get; set; }

        protected void BaseAwake((int width, int height) topSize, (int width, int height) rightsize) {
            this.Top = new Rect(topSize.width, topSize.height);
            this.Bottom = new Rect(topSize.width, -topSize.height);
            this.Left = new Rect(-rightsize.width, rightsize.height);
            this.Right = new Rect(rightsize.width, rightsize.height);

            this.Top.LocalPos = PlayerCombat.AttackPointTop;
            this.Top.LocalPos += new Vector2(0f, this.Top.height / 2);

            this.Bottom.LocalPos = PlayerCombat.AttackPointBottom;
            this.Bottom.LocalPos += new Vector2(0f, this.Bottom.height / 2);

            this.Left.LocalPos = PlayerCombat.AttackPointLeft;
            this.Left.LocalPos += new Vector2(this.Left.width / 4, 0f);


            this.Right.LocalPos = PlayerCombat.AttackPointRight;
            this.Right.LocalPos += new Vector2(this.Right.width / 4, 0f);

            this.HitBox = new WeaponHitBox(this.Top, this.Bottom, this.Left, this.Right);
        }
    }
}
