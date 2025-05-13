using System;
using JetBrains.Annotations;
using Treep.Player;
using UnityEngine;

namespace Treep.Weapon {
    public class NoHitboxCloseWeapon : MonoBehaviour, ICloseWeapon {
        public string Name { get; set; }
        public int Damage { get; set; }
        public float AttackRate { get; set; }
        public WeaponHitBox HitBox { get; set; } = null;
        protected CircleCollider2D Top { get; set; }
        protected CircleCollider2D Bottom { get; set; }
        protected CircleCollider2D Left { get; set; }
        protected CircleCollider2D Right { get; set; }


        protected void BaseAwake(float legacyRange) {
            // BaseAwake set Top, Bottom, Left, Right and Hitbox

            this.Top = this.gameObject.AddComponent<CircleCollider2D>();
            this.Bottom = this.gameObject.AddComponent<CircleCollider2D>();
            this.Left = this.gameObject.AddComponent<CircleCollider2D>();
            this.Right = this.gameObject.AddComponent<CircleCollider2D>();

            //this.Top.id
            this.Top.radius = legacyRange;
            this.Top.offset = PlayerCombat.AttackPointTop;
            //this.Top.offset = Vector3.zero;
            this.Top.isTrigger = true;

            this.Bottom.radius = legacyRange;
            this.Bottom.offset = PlayerCombat.AttackPointBottom;
            //this.Bottom.offset = Vector3.zero;
            this.Bottom.isTrigger = true;

            this.Left.radius = legacyRange;
            this.Left.offset = PlayerCombat.AttackPointLeft;
            //this.Left.offset = Vector3.zero;
            this.Left.isTrigger = true;


            this.Right.radius = legacyRange;
            this.Right.offset = PlayerCombat.AttackPointRight;
            //this.Right.offset = Vector3.zero;
            this.Right.isTrigger = true;

            Debug.Log($"Top.pos = {this.Top.transform.localPosition}\n" +
                      $"Bottom.pos = {this.Bottom.transform.localPosition}\n" +
                      $"Left.pos = {this.Left.transform.localPosition}\n" +
                      $"Right.pos = {this.Right.transform.localPosition}\n");

            this.HitBox = new WeaponHitBox(this.Top, this.Bottom, this.Left, this.Right);
        }


        public override string ToString() {
            return $"Name : {this.Name}\n" +
                   $"Damage : {this.Damage}\n" +
                   $"AttackRate : {this.AttackRate}\n";
        }
    }
}
