using System;
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

        private static readonly int IsCloseAttacking = Animator.StringToHash("isCloseAttacking");
        private static readonly int Random = Animator.StringToHash("Random");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int IsDistAttacking = Animator.StringToHash("isDistAttacking");

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

        private LookDirection _currentLookDirection;

        private float nextAttackTime;
        private int Health { get; set; }


        public void Awake() {
            this.ControllerScript = this.GetComponent<PlayerController>();
            this._currentLookDirection = this.ControllerScript.lookDirection;
            this.Health = this.MaxPV;
            this._weaponManager = this.gameObject.GetComponent<WeaponManager>();


            this._animator = this.GetComponent<Animator>();
            this._attackAnimator = this.attackPoint.GetComponent<Animator>();
        }

        public void SwitchWeapon(Weapons newWeapon) {
            this._weaponManager.SwitchWeapon(newWeapon);
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
            this._currentLookDirection = this.ControllerScript.lookDirection;
            switch (this._currentLookDirection) {
                case LookDirection.Right:
                    this.attackPoint.position = PlayerCombat.AttackPointRight + (Vector2)this.transform.position;
                    break;
                case LookDirection.Left:
                    this.attackPoint.position = PlayerCombat.AttackPointLeft + (Vector2)this.transform.position;
                    break;
                case LookDirection.Top:
                    this.attackPoint.position = PlayerCombat.AttackPointTop + (Vector2)this.transform.position;
                    break;
                case LookDirection.Bottom:
                    this.attackPoint.position = PlayerCombat.AttackPointBottom + (Vector2)this.transform.position;
                    break;
            }


            this._weaponManager.UpdateLooking(this._currentLookDirection);
        }

        private void CloseAttack() {
            this._animator.SetTrigger(PlayerCombat.IsCloseAttacking);

            this._attackAnimator.SetInteger(PlayerCombat.Random, new Random().Next(6));
            this._attackAnimator.SetTrigger(PlayerCombat.Attack);


            // check enemy
            var enemiesToHit = this.GetEnemyIn(this._weaponManager.HitBox);
            // Damage to enemy
            foreach (var enemy in enemiesToHit) {
                Debug.Log($"{enemy.name} took {this._weaponManager.Damage} damage");
                enemy.GetComponent<IEnemy>().Hit(this._weaponManager.Damage);
            }
        }

        private void DistAttack() {
            this._animator.SetTrigger(PlayerCombat.IsDistAttacking);
        }

        private Collider2D[] GetEnemyIn(IShapesHitbox shape) {
            var results = new Collider2D[50];
            var count = 0;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(this.enemyLayerMask);
            filter.useTriggers = true;

            if (shape is Circle circle) {
                count = Physics2D.OverlapCircle(shape.GetGlobalPosition(this.transform.position), circle.Radius,
                    filter, results);
            }
            else if (shape is Rect rect) {
                count = Physics2D.OverlapBox(shape.GetGlobalPosition(this.transform.position), rect.Size, 0f, filter,
                    results);
            }
            else {
                Debug.LogWarning("unknown shape");
            }

            return results.Take(count).ToArray();
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointTop + this.transform.position, 0.1f);
            Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointBottom + this.transform.position, 0.1f);
            Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointRight + this.transform.position, 0.1f);
            Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointLeft + this.transform.position, 0.1f);

            Gizmos.color = Color.black;

            switch (this._currentLookDirection) {
                case LookDirection.Top:
                    Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointTop + this.transform.position, 0.1f);
                    break;
                case LookDirection.Bottom:
                    Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointBottom + this.transform.position, 0.1f);
                    break;
                case LookDirection.Right:
                    Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointRight + this.transform.position, 0.1f);
                    break;
                case LookDirection.Left:
                    Gizmos.DrawSphere((Vector3)PlayerCombat.AttackPointLeft + this.transform.position, 0.1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
