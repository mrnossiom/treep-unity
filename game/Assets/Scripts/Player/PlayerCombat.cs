using System;
using System.Linq;
using Mirror;
using Treep.IA;
using Treep.SFX;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Audio;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using Rect = Treep.Weapon.Rect;

namespace Treep.Player {
    public class PlayerCombat : NetworkBehaviour {
        public static Vector2 AttackPointRight = new(1.5f, 0f);
        public static Vector2 AttackPointLeft = new(-1.5f, 0f);
        public static Vector2 AttackPointTop = new(0f, 1.75f);
        public static Vector2 AttackPointBottom = new(0f, -1.75f);

        public Transform attackPoint;
        public LayerMask enemyLayerMask;

        public KeyCode closeAttackKey = KeyCode.L;

        private static readonly int IsCloseAttacking = Animator.StringToHash("isCloseAttacking");
        private static readonly int Random = Animator.StringToHash("Random");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int IsDistAttacking = Animator.StringToHash("isDistAttacking");

        private Animator _attackAnimator;
        private WeaponManager _weaponManager;
        private PlayerController _controllerScript;
        private PlayerAnimatorController _animatorController;
        private LookDirection _currentLookDirection;

        private float _nextAttackTime;

        private int _money {
            get => this.GetComponent<Player>().money;
            set => this.GetComponent<Player>().money = value;
        }

        [SerializeField] private AudioClip slashSoundClip;
        [SerializeField] private AudioMixer audioMixer;

        public void Awake() {
            this._controllerScript = this.GetComponent<PlayerController>();
            this._currentLookDirection = this._controllerScript.lookDirection;
            this._weaponManager = this.gameObject.GetComponent<WeaponManager>();
            this._animatorController = this.GetComponent<PlayerAnimatorController>();
            this._attackAnimator = this.attackPoint.GetComponent<Animator>();
        }

        public void SwitchWeapon(Weapons newWeapon) {
            this._weaponManager.SwitchWeapon(newWeapon);
        }

        public void Update() {
            if (!this.isLocalPlayer) return;

            this.UpdateAttackPoint();

            if (Time.time >= this._nextAttackTime) {
                if (Input.GetKeyDown(this.closeAttackKey)) {
                    CloseAttack();
                    this._nextAttackTime = Time.time + 1f / this._weaponManager.AttackRate;
                }
            }
        }

        private void UpdateAttackPoint() {
            this._currentLookDirection = this._controllerScript.lookDirection;
            switch (this._currentLookDirection) {
                case LookDirection.Right:
                    this.attackPoint.position = AttackPointRight + (Vector2)this.transform.position;
                    break;
                case LookDirection.Left:
                    this.attackPoint.position = AttackPointLeft + (Vector2)this.transform.position;
                    break;
                case LookDirection.Top:
                    this.attackPoint.position = AttackPointTop + (Vector2)this.transform.position;
                    break;
                case LookDirection.Bottom:
                    this.attackPoint.position = AttackPointBottom + (Vector2)this.transform.position;
                    break;
            }

            this._weaponManager.UpdateLooking(this._currentLookDirection);
        }

        private void CloseAttack() {
            if (!this.isLocalPlayer) return;
            
            this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
            soundLevel = (soundLevel + 80) / 100;
            SoundFXManager.Instance.PlaySoundFXClip(this.slashSoundClip, this.transform, soundLevel);
            
            CmdPerformAttack(this._controllerScript.lookDirection);
        }

        [Command]
        private void CmdPerformAttack(LookDirection direction) {
            RpcPlayAttackAnimation(direction);
            
            var enemiesToHit = GetEnemyIn(this._weaponManager.HitBox);
            foreach (var enemy in enemiesToHit) {
                this._money += enemy.GetComponent<IEnemy>().Hit(this._weaponManager.Damage);
            }
        }

        [ClientRpc]
        private void RpcPlayAttackAnimation(LookDirection direction) {
            if (!this.isActiveAndEnabled) return;

            this._animatorController.TriggerAttack(direction);
            this._attackAnimator.SetInteger(Random, new Random().Next(6));
            this._attackAnimator.SetTrigger(Attack);
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
            Debug.Log("Found " + count + " enemy in the attack");
            return results.Take(count).ToArray();
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere((Vector3)AttackPointTop + this.transform.position, 0.1f);
            Gizmos.DrawSphere((Vector3)AttackPointBottom + this.transform.position, 0.1f);
            Gizmos.DrawSphere((Vector3)AttackPointRight + this.transform.position, 0.1f);
            Gizmos.DrawSphere((Vector3)AttackPointLeft + this.transform.position, 0.1f);

            Gizmos.color = Color.black;

            switch (this._currentLookDirection) {
                case LookDirection.Top:
                    Gizmos.DrawSphere((Vector3)AttackPointTop + this.transform.position, 0.1f);
                    break;
                case LookDirection.Bottom:
                    Gizmos.DrawSphere((Vector3)AttackPointBottom + this.transform.position, 0.1f);
                    break;
                case LookDirection.Right:
                    Gizmos.DrawSphere((Vector3)AttackPointRight + this.transform.position, 0.1f);
                    break;
                case LookDirection.Left:
                    Gizmos.DrawSphere((Vector3)AttackPointLeft + this.transform.position, 0.1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}