using System.Collections.Generic;
using System.Linq;
using Treep.IA;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using Rect = Treep.Weapon.Rect;

namespace Treep.Player {
    public class PlayerCombat : MonoBehaviour {
        public static Vector2 AttackPointRight = new(1.5f, 0f);
        public static Vector2 AttackPointLeft = new(-1.5f, 0f);
        public static Vector2 AttackPointTop = new(0f, 1.75f);
        public static Vector2 AttackPointBottom = new(0f, -1.75f);
        public Transform attackPoint;
        private Animator _attackAnimator;

        [FormerlySerializedAs("ennemyLayerMask")]
        public LayerMask enemyLayerMask;

        public List<GameObject> weapons;
        public KeyCode CloseAttackKey = KeyCode.L;
        public KeyCode DistAttackKey = KeyCode.O;

        [FormerlySerializedAs("PVMax")] public int MaxPV = 10;
        private Animator _animator;

        private WeaponManager _weaponManager;
        private PlayerController ControllerScript;

        private Looking currentLooking;

        private float nextAttackTime;
        private int Pv { get; set; }


        public void Awake() {
            this.ControllerScript = this.GetComponent<PlayerController>();
            this.currentLooking = this.ControllerScript.looking;
            this.Pv = this.MaxPV;
            this._weaponManager = this.gameObject.GetComponent<WeaponManager>();

            this._animator = this.GetComponent<Animator>();
            this._attackAnimator = this.attackPoint.GetComponent<Animator>();
        }

        public void Update() {
            this.UpdateAttackPoint();

            if (Time.time >= this.nextAttackTime) {
                if (Input.GetKeyDown(this.CloseAttackKey)) {
                    this.CloseAttack();
                    this.nextAttackTime = Time.time + 1f / this._weaponManager.AttackRate;
                }
            }
        }


        private void UpdateAttackPoint() {
            this.currentLooking = this.ControllerScript.looking;
            switch (this.currentLooking) {
                case Looking.Right:
                    this.attackPoint.position = PlayerCombat.AttackPointRight + (Vector2)this.transform.position;
                    break;
                case Looking.Left:
                    this.attackPoint.position = PlayerCombat.AttackPointLeft + (Vector2)this.transform.position;
                    break;
                case Looking.Top:
                    this.attackPoint.position = PlayerCombat.AttackPointTop + (Vector2)this.transform.position;
                    break;
                case Looking.Bottom:
                    this.attackPoint.position = PlayerCombat.AttackPointBottom + (Vector2)this.transform.position;
                    break;
            }


            this._weaponManager.UpdateLooking(this.currentLooking);
        }

        private void CloseAttack() {
            // animation
            this._animator.SetTrigger("isCloseAttacking");

            this._attackAnimator.SetInteger("Random", new Random().Next(6));
            this._attackAnimator.SetTrigger("Attack");


            //check eneny
            var hitEnemys = this.GetEnnemyIn(this._weaponManager.HitBox);
            //Damage to ennemy
            foreach (var ennemy in hitEnemys) {
                Debug.Log($"{ennemy.name} took {this._weaponManager.Damage} damage");
                ennemy.GetComponent<IEnemy>().GetHitted(this._weaponManager.Damage);
            }
        }

        private void DistAttack() {
            this._animator.SetTrigger("isDistAttacking");
        }

        public Collider2D[] GetEnnemyIn(IShapesHitbox shape) {
            var results = new Collider2D[50];
            var count = 0;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(this.enemyLayerMask);
            filter.useTriggers = true;

            if (shape is Circle circle) {
                count = Physics2D.OverlapCircle(circle.GetPos(this.transform.position), circle.Radius, filter, results);
            }
            else if (shape is Rect rect) {
                count = Physics2D.OverlapBox(rect.GetPos(this.transform.position), rect.Size, 0f, filter, results);
            }
            else {
                Debug.LogWarning("unknown shape");
            }

            return results.Take(count).ToArray();
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointTop) + this.transform.position, 0.1f);
            Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointBottom) + this.transform.position, 0.1f);
            Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointRight) + this.transform.position, 0.1f);
            Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointLeft) + this.transform.position, 0.1f);

            Gizmos.color = Color.black;

            switch (this.currentLooking) {
                case Looking.Top:
                    Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointTop) + this.transform.position, 0.1f);
                    break;
                case Looking.Bottom:
                    Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointBottom) + this.transform.position, 0.1f);
                    break;
                case Looking.Right:
                    Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointRight) + this.transform.position, 0.1f);
                    break;
                case Looking.Left:
                    Gizmos.DrawSphere(this.To3D(PlayerCombat.AttackPointLeft) + this.transform.position, 0.1f);
                    break;
            }
        }

        public Vector3 To3D(Vector2 pos) {
            return new Vector3(pos.x, pos.y, 0f);
        }
    }
}
