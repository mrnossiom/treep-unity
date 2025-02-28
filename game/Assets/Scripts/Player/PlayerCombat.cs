using Treep.IA;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace Treep.Player {
    public class PlayerCombat : MonoBehaviour {
        // Attack
        public Transform attackPointRight;
        public Transform attackPointLeft;
        public Transform attackPointTop;
        public Transform attackPointBottom;
        public Vector2 attackPoint;

        [FormerlySerializedAs("ennemyLayerMask")]
        public LayerMask enemyLayerMask;

        private float nextAttackTime = 0;
        private Controller ControllerScript;
        private Looking currentLooking;

        public int PVMax = 10;
        private int PV { get; set; }

        private ICloseWeapon _currentCloseWeapon;
        private Animator _animator;


        public void Awake() {
            this.ControllerScript = this.GetComponent<Controller>();
            this.currentLooking = this.ControllerScript.looking;
            this.PV = this.PVMax;
            this._currentCloseWeapon = new Stick();
            this._animator = this.GetComponent<Animator>();
        }

        public void Update() {
            this.UpdateAttackPoint();
            if (Time.time >= this.nextAttackTime) {
                if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
                    this.Attack();
                    this.nextAttackTime = Time.time + 1f / this._currentCloseWeapon.AttackRate;
                }
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                Debug.Log($"{this._currentCloseWeapon}");
                Debug.Log($"{this.PV}");
                Debug.Log(this.attackPoint);
            }
        }


        private void UpdateAttackPoint() {
            this.currentLooking = this.ControllerScript.looking;
            switch (this.currentLooking) {
                case Looking.Right:
                    this.attackPoint = this.attackPointRight.position;
                    break;
                case Looking.Left:
                    this.attackPoint = this.attackPointLeft.position;
                    break;
                case Looking.Top:
                    this.attackPoint = this.attackPointTop.position;
                    break;
                case Looking.Bottom:
                    this.attackPoint = this.attackPointBottom.position;
                    break;
            }
        }

        private void Attack() {
            // animation
            this._animator.SetTrigger("isAttacking");

            //check enemy
            var hitEnnemys = Physics2D.OverlapCircleAll(this.attackPoint, this._currentCloseWeapon.AttackRange,
                this.enemyLayerMask);
            //Damage to ennemy
            foreach (var ennemy in hitEnnemys) {
                Debug.Log($"{ennemy.name} took {this._currentCloseWeapon.Damage} damage");
                ennemy.GetComponent<IEnemy>().GetHitted(this._currentCloseWeapon.Damage);
            }
        }

        public void OnDrawGizmosSelected() {
            if (this.attackPointRight == null
                || this.attackPointLeft == null
                || this.attackPointTop == null
                || this.attackPointBottom == null) {
                return;
            }

            //Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
            //Gizmos.DrawWireSphere(attackPointLeft.position, attackRange);
            //Gizmos.DrawWireSphere(attackPointTop.position, attackRange);
            //Gizmos.DrawWireSphere(attackPointBottom.position, attackRange);
            Gizmos.DrawWireSphere(this.attackPoint, this._currentCloseWeapon.AttackRange);
        }
    }
}
