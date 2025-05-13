using System.Diagnostics;
using System.Linq;
using Treep.IA;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = System.Random;

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
            this._weaponManager = this.gameObject.AddComponent<WeaponManager>();
            this._animator = this.GetComponent<Animator>();
            this._attackAnimator = this.attackPoint.GetComponent<Animator>();
        }

        public void Update() {
            this.UpdateAttackPoint();

            if (Time.time >= this.nextAttackTime) {
                if (Input.GetKeyDown(this.CloseAttackKey)) {
                    this.CloseAttack();
                    this.nextAttackTime = Time.time + 1f / this._weaponManager.Weapon.AttackRate;
                }
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                Debug.Log($"{this._weaponManager.Weapon}");
                Debug.Log($"{this.Pv}");
                Debug.Log(this.attackPoint.position);
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

            if (this._weaponManager == null) {
                Debug.Log("this._weaponManager");
            }

            if (this._weaponManager.Weapon == null) {
                Debug.Log("this._weaponManager.Weapon");
            }

            if (this._weaponManager.Weapon.HitBox == null) {
                Debug.Log("this._weaponManager.Weapon.HitBox");
            }

            this._weaponManager.Weapon.HitBox?.UpdateLooking(this.currentLooking);
        }

        private void CloseAttack() {
            // animation
            this._animator.SetTrigger("isCloseAttacking");

            this._attackAnimator.SetInteger("Random", new Random().Next(6));
            this._attackAnimator.SetTrigger("Attack");


            //check enemy
            if (this._weaponManager.Weapon.HitBox.Current == null) {
                Debug.Log(
                    "this._currentCloseWeapon.HitBox.Current est null (SUS)" +
                    this._weaponManager.Weapon.ToString());
            }

            var hitEnnemys = this.GetEnnemyIn(this._weaponManager.Weapon.HitBox.Current);
            //Damage to ennemy
            foreach (var ennemy in hitEnnemys) {
                Debug.Log($"{ennemy.name} took {this._weaponManager.Weapon.Damage} damage");
                ennemy.GetComponent<IEnemy>().GetHitted(this._weaponManager.Weapon.Damage);
            }
        }

        private void DistAttack() {
            this._animator.SetTrigger("isDistAttacking");
        }

        public Collider2D[] GetEnnemyIn(Collider2D areaCollider) {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(this.enemyLayerMask);
            var results = new Collider2D[50]; // Taille max des résultats
            var count = areaCollider.Overlap(filter, results);
            // Si tu veux exactement le tableau des résultats trouvés
            return results.Take(count).ToArray();
        }
    }
}
