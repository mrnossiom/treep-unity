using System.Diagnostics;
using Treep.IA;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Treep.Player {
    public class PlayerCombat : MonoBehaviour {
        public Vector2 attackPointRight;
        public Vector2 attackPointLeft;
        public Vector2 attackPointTop;
        public Vector2 attackPointBottom;
        public Transform attackPoint;
        private Animator _attackAnimator;

        [FormerlySerializedAs("ennemyLayerMask")]
        public LayerMask enemyLayerMask;

        public KeyCode CloseAttackKey = KeyCode.L;
        public KeyCode DistAttackKey = KeyCode.O;

        [FormerlySerializedAs("PVMax")] public int MaxPV = 10;
        private Animator _animator;

        private ICloseWeapon _currentCloseWeapon;
        private PlayerController ControllerScript;

        private Looking currentLooking;

        private float nextAttackTime;
        private int Pv { get; set; }


        public void Awake() {
            this.ControllerScript = this.GetComponent<PlayerController>();
            this.currentLooking = this.ControllerScript.looking;
            this.Pv = this.MaxPV;
            this._currentCloseWeapon = new Stick();
            this._animator = this.GetComponent<Animator>();
            this._attackAnimator = this.attackPoint.GetComponent<Animator>();
        }

        public void Update() {
            this.UpdateAttackPoint();

            if (Time.time >= this.nextAttackTime) {
                if (Input.GetKeyDown(this.CloseAttackKey)) {
                    this.CloseAttack();
                    this.nextAttackTime = Time.time + 1f / this._currentCloseWeapon.AttackRate;
                }
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                Debug.Log($"{this._currentCloseWeapon}");
                Debug.Log($"{this.Pv}");
                Debug.Log(this.attackPoint.position);
            }
        }

        public void OnDrawGizmosSelected() {
            if (this.attackPoint == null) {
                return;
            }

            Debug.Log(this._currentCloseWeapon);
            Debug.Log(this._animator);

            Gizmos.DrawWireSphere(this.attackPoint.position, 1); // this._currentCloseWeapon.AttackRange);
        }

        private void UpdateAttackPoint() {
            this.currentLooking = this.ControllerScript.looking;
            switch (this.currentLooking) {
                case Looking.Right:
                    this.attackPoint.position = this.attackPointRight + (Vector2)this.transform.position;
                    break;
                case Looking.Left:
                    this.attackPoint.position = this.attackPointLeft + (Vector2)this.transform.position;
                    break;
                case Looking.Top:
                    this.attackPoint.position = this.attackPointTop + (Vector2)this.transform.position;
                    break;
                case Looking.Bottom:
                    this.attackPoint.position = this.attackPointBottom + (Vector2)this.transform.position;
                    break;
            }
        }

        private void CloseAttack() {
            // animation
            this._animator.SetTrigger("isCloseAttacking");

            this._attackAnimator.SetInteger("Random", new Random().Next(6));
            this._attackAnimator.SetTrigger("Attack");

            //check enemy
            var hitEnnemys = Physics2D.OverlapCircleAll(this.attackPoint.position, this._currentCloseWeapon.AttackRange,
                this.enemyLayerMask);
            //Damage to ennemy
            foreach (var ennemy in hitEnnemys) {
                Debug.Log($"{ennemy.name} took {this._currentCloseWeapon.Damage} damage");
                ennemy.GetComponent<IEnemy>().GetHitted(this._currentCloseWeapon.Damage);
            }
        }

        private void DistAttack() {
            this._animator.SetTrigger("isDistAttacking");
        }
    }
}
