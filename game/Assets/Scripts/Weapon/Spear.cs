using System;
using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class Spear : MonoBehaviour, ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Durability { get; set; }
        public float AttackRange { get; set; }
        public WeaponHitBox HitBox { get; set; }
        public float AttackRate { get; set; }

        public BoxCollider2D HitboxTop;
        public BoxCollider2D HitboxBottom;
        public BoxCollider2D HitboxLeft;
        public BoxCollider2D HitboxRight;

        private void Awake() {
            this.Name = "Spear";
            this.Damage = 1;
            this.Durability = 1;
            this.AttackRange = 0.0f;
            this.AttackRate = 0.2f;

            // Modify HitBox of the Weapon
            // this.HitboxTop = new BoxCollider2D();
            // this.HitboxTop.transform.position = PlayerCombat.AttackPointTop;
            // this.HitboxTop.transform.position -= new Vector3(0.5f, 0f, 0f); // car size.x = 1
            // this.HitboxTop.size = new Vector2(1, 3);
            //
            // this.HitboxBottom = new BoxCollider2D();
            // this.HitboxBottom.transform.position = PlayerCombat.AttackPointBottom;
            // this.HitboxTop.transform.position -= new Vector3(0.5f, 0f, 0f); // car size.x = 1
            // this.HitboxBottom.size = new Vector2(1, -3);
            //
            //
            // this.HitboxLeft = new BoxCollider2D();
            // this.HitboxLeft.transform.position = PlayerCombat.AttackPointLeft;
            // this.HitboxTop.transform.position -= new Vector3(0f, 0.5f, 0f); // car size.y = 1
            // this.HitboxBottom.size = new Vector2(-3, 1);
            //
            // this.HitboxRight = new BoxCollider2D();
            // this.HitboxRight.transform.position = PlayerCombat.AttackPointRight;
            // this.HitboxTop.transform.position -= new Vector3(0f, 0.5f, 0f); // car size.y = 1
            // this.HitboxBottom.size = new Vector2(3, 1);

            this.HitBox = new WeaponHitBox(this.HitboxTop, this.HitboxBottom, this.HitboxLeft, this.HitboxRight);
        }
    }
}
