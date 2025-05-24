using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Pathfinding;
using Treep.Player;
using Treep.SFX;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Treep.IA {
    public class BasicEnemy : MonoBehaviour, IEnemy {
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
        public float PV { get; set; }
        
        public Rigidbody2D body;
        //public Collider2D _collider2d;
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
            
            //this.InvokeRepeating(nameof(this.OnDebug), 0, 2.5f);
            
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
            //this._contactFilter.useLayerMask = true;
            //this._contactFilter.layerMask = LayerMask.GetMask("Wall");
        }

        private void FixedUpdate() {
            if (this._moveEnabled && this.isAlive && this.isTriggerByPlayer) {
                this.UpdatePathFinding();
                this.UpdateJump();
                this.animator.SetBool(BasicEnemy.GetHit, false);
                
                if (this.body.linearVelocity.x > 0) {
                    this.spriteRenderer.flipX = false;
                }
                else {
                    this.spriteRenderer.flipX = true;
                }
            }
        }
        
        public void Hit(float damageTook) {
            this.PV -= damageTook;

            if (this.PV <= 0) {
                this.Die();
            }
            else {
                this.animator.SetBool(BasicEnemy.GetHit, true);
                audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.damageSoundClip, this.transform,soundLevel);
            }
        }

        private bool IsWall() {
            var hits = new RaycastHit2D[16];
            var count = this.body.Cast(this._moveDirection, hits, this.detectionDistance);

            return count > 0;
        }

        public void Die() {
            this.isAlive = false;
            // Die animation 
            
            this.spriteRenderer.enabled = false;
            this._collider.enabled = false;
            this.enabled = false;
            
            audioMixer.GetFloat("SFXVolume", out var soundLevel);
            soundLevel = (soundLevel + 80) / 100;
            SoundFXManager.Instance.PlaySoundFXClip(this.deathSoundClip, this.transform,soundLevel);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.body.position, this.radiusDetectPlayer);
        }
        
        private void OnDebug() {
            Debug.Log($"isTriggerByPlayer: {this.isTriggerByPlayer}");
        }
    }
}
