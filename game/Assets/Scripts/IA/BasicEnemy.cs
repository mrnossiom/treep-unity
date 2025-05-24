using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using Pathfinding;
using Treep.Player;
using Treep.SFX;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
using Rect = UnityEngine.Rect;

namespace Treep.IA {
    public class BasicEnemy : NetworkBehaviour {
        private static readonly int GetHit = Animator.StringToHash("GetHit");

        public int PVMax;
        public float maxspeed = 4;
        public float detectionDistance = 0.2f;
        public float speed = 30f;
        public float nexWaypointDistance = 3f;
        public bool isTriggerByPlayer = false;
        public int radiusDetectPlayer = 20;
        public float delayDetectPlayer = 1f;
        public bool isAlive = true;
        public int loot = 2;
        public float damage = 5f;
        public Vector2 hitBoxToPlayer = new Vector2(2f, 3.5f);

        [SyncVar(hook = nameof(OnPVChanged))]
        private float _pv;

        public float PV {
            get => _pv;
            set {
                if (isServer) {
                    _pv = value;
                }
            }
        }

        public Rigidbody2D body;
        public SpriteRenderer spriteRenderer;
        public Animator animator;
        public LayerMask playerLayerMask;

        private Collider2D _collider;
        private Seeker _seeker;

        private readonly bool _moveEnabled = true;
        private int _currentWaypoint = 0;
        private bool _reachedEndOfPath = false;
        private float _jumpDeltaTime;
        private float _jumpDelay = 1f;
        
        private float _attackDeltaTime;
        private float _attackDelay = 1f;

        private readonly Vector2 _moveDirection = Vector2.right;
        private Transform Target => Player.Player.Singleton.gameObject.transform;
        private Path _currentPath;
        private Vector2 _velocity;

        [SerializeField] private AudioClip damageSoundClip;
        [SerializeField] private AudioClip deathSoundClip;
        [SerializeField] private AudioMixer audioMixer;

        public void Awake() {
            this.body = this.GetComponent<Rigidbody2D>();
            this._seeker = this.GetComponent<Seeker>();
            this._collider = this.GetComponent<Collider2D>();

            this.InvokeRepeating(nameof(this.UpdatePath), 0, 0.25f);
            this.InvokeRepeating(nameof(this.DetectPlayer), 0, this.delayDetectPlayer);
        }

        private void DetectPlayer() {
            var results = new Collider2D[50];
            var count = 0;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(this.playerLayerMask);
            filter.useTriggers = true;

            count = Physics2D.OverlapCircle(this.body.position, this.radiusDetectPlayer,
                filter, results);

            this.isTriggerByPlayer = count > 0;
        }

        private void UpdatePath() {
            if (this.isAlive && this.isTriggerByPlayer) {
                if (this._seeker.IsDone())
                    this._seeker.StartPath(this.body.position, this.Target.position, this.OnPathComplete);
            }
        }

        private void OnPathComplete(Path path) {
            if (!path.error) {
                _currentPath = path;
                _currentWaypoint = 0;
            }
        }

        private void UpdatePathFinding() {
            if (this._currentPath is null) return;

            if (this._currentWaypoint >= this._currentPath.vectorPath.Count) {
                this._reachedEndOfPath = true;
                return;
            }
            this._reachedEndOfPath = false;

            Vector2 direction = ((Vector2)this._currentPath.vectorPath[this._currentWaypoint] - this.body.position).normalized;
            this._velocity = direction * (this.speed * Time.deltaTime);

            this.body.linearVelocity += new Vector2(this._velocity.x, 0);
            if (this.body.linearVelocity.x > maxspeed)
                this.body.linearVelocity = new Vector2(maxspeed, this.body.linearVelocity.y);
            else if (this.body.linearVelocity.x < -maxspeed)
                this.body.linearVelocity = new Vector2(-maxspeed, this.body.linearVelocity.y);

            float distance = Vector2.Distance(this.body.position, this._currentPath.vectorPath[this._currentWaypoint]);

            if (distance < this.nexWaypointDistance) {
                this._currentWaypoint += 1;
            }
        }

        private void UpdateJump() {
            if (this.body.linearVelocity.x == 0 &&
                Time.time - this._jumpDeltaTime > this._jumpDelay &&
                !this._reachedEndOfPath &&
                Math.Abs(this.Target.position.x - this.body.position.x) > 5) {
                this.Jump();
            }
        }

        private void Jump() {
            this._jumpDeltaTime = Time.time;
            this.body.linearVelocity += Vector2.up * 7;
        }

        private void Start() {
            this.PV = this.PVMax;
        }

        private void FixedUpdate() {
            if (!isServer) return;

            if (this._moveEnabled && this.isAlive && this.isTriggerByPlayer) {
                this.UpdatePathFinding();
                this.UpdateJump();
                this.UpdateAttack();
                this.animator.SetBool(BasicEnemy.GetHit, false);

                if (this.body.linearVelocity.x > 0) {
                    this.spriteRenderer.flipX = false;
                }
                else {
                    this.spriteRenderer.flipX = true;
                }
            }
        }
        
        public int Hit(float damageTook) {
            if (!isServer) return 0;

            this.PV -= damageTook;

            if (this.PV <= 0) {
                this.Die();
                return this.loot;
            }
            else {
                RpcPlayHitAnimationAndSound();
                return 0;
            }
        }
        
        private void OnPVChanged(float oldPV, float newPV) {
            this.PV = newPV;
        }

        [ClientRpc]
        private void RpcPlayHitAnimationAndSound() {
            if (!this.isActiveAndEnabled) return;
            this.animator.SetBool(BasicEnemy.GetHit, true);

            audioMixer.GetFloat("SFXVolume", out var soundLevel);
            soundLevel = (soundLevel + 80) / 100;
            SoundFXManager.Instance.PlaySoundFXClip(this.damageSoundClip, this.transform, soundLevel);
        }

        public void UpdateAttack() {
            if (Time.time - this._attackDeltaTime > this._attackDelay) {
                Player.Player[] players = DetectPlayerIsCollidingZone();
                if (players.Length > 0) {
                    this._attackDeltaTime = Time.time;
                    this.Attack(players);
                }
            }
            
        }

        public void Attack(Player.Player[] players) {
            foreach (var player in players) {
                this.GiveDamage(player);
            }
        }


        public Player.Player[] DetectPlayerIsCollidingZone() {
        
            var results = new Collider2D[50];
            var count = 0;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(this.playerLayerMask);
            filter.useTriggers = true;

            
            count = Physics2D.OverlapBox(this.body.position, this.hitBoxToPlayer, 0f, filter, results);

            
            return (from collider2d in results.Take(count)
                select collider2d.GetComponent<Player.Player>()).ToArray();
        }

        public void GiveDamage(Player.Player player) {
            player.TakeDamage(this.damage);
        }

        private bool IsWall() {
            var hits = new RaycastHit2D[16];
            var count = this.body.Cast(this._moveDirection, hits, this.detectionDistance);

            return count > 0;
        }

        public void Die() {
            this.isAlive = false;
            this.RpcDie();
            //Object.Destroy(this);
        }

        

        [ClientRpc]
        private void RpcDie() {
            if (!this.isActiveAndEnabled) return;

            this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
            soundLevel = (soundLevel + 80) / 100;
            SoundFXManager.Instance.PlaySoundFXClip(this.deathSoundClip, this.transform, soundLevel);
            
            this.animator.SetTrigger("Die");
            this.body.simulated = false;
            //this.Invoke(nameof(this.InvokeDie), 5f);
        }
        
        private void InvokeDie() {
            Debug.Log("Et boum il est DESTROY");

            Object.Destroy(this.gameObject);
        }

        private void OnDrawGizmos() {
            if (this.body == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.body.position, this.radiusDetectPlayer);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(this.body.position, this.hitBoxToPlayer);


            
        }
    }
}
