using System;
using Treep.IA;
using UnityEngine;
using Mirror;
using UnityEngine.Serialization;

namespace Treep.IA
{
    using UnityEngine;
    using Mirror;

    public class BossFight : NetworkBehaviour , IEnemy {
        private static readonly int GetHitted = Animator.StringToHash("GetHitted");
        private static readonly int Vulnerable = Animator.StringToHash("Vulnerable");
        private static readonly int IsAlive = Animator.StringToHash("IsAlive");
        public int pvMax = 100;
        public bool isAlive;
        public GameObject objectToSpawn;
        public GameObject specialObjectToSpawn;
        public float minXDistance = 3f;
        public float maxXDistance = 10f;
        public float spawnHeight = 10f;
        [SyncVar(hook = nameof(OnPVChanged))] 
        private float _pv;

        public float PV {
            get => this._pv;
            set {
                if (isServer) {
                    _pv = value;
                }
            }
        }

        public Rigidbody2D body;
        public SpriteRenderer spriteRenderer;
        private Animator _animator;
        private Collider2D _collider;
        

        private float _fightTimer = 0f;
        private float _nextSpawnTimeRoll = 0f;
        private float _nextSpawnTimeFall = 0f;
        private float _secondPhaseTimer = 0f;
        private float _thirdPhaseTimer = 0f;
        private bool _vulnerable = false;

        public void Awake() {
            this.body = this.GetComponent<Rigidbody2D>();
            this._collider = this.GetComponent<Collider2D>();
            this._animator = this.GetComponent<Animator>();
        }

        private void Start() {
            this.PV = this.pvMax;
        }

        void Update()
        {
            this._fightTimer += Time.deltaTime;

            if (this._fightTimer <= 20f)
            {
                if (Time.time >= this._nextSpawnTimeFall)
                {
                    SpawnFallingRock();
                    this._nextSpawnTimeFall = Time.time + Random.Range(0.2f, 2f);
                }
            }

            if ((this._fightTimer > 20f && this._fightTimer < 29f) || (this._fightTimer > 50f && this._fightTimer < 59f) || this._fightTimer > 80f) {
                this._vulnerable = true;
                this._animator.SetBool(Vulnerable, true);
            }

            if (this._fightTimer < 20f || (this._fightTimer > 29f && this._fightTimer < 50f) || (this._fightTimer > 59f && this._fightTimer < 80f)) {
                this._vulnerable = false;
                this._animator.SetBool(Vulnerable, false);
            }
            else if (this._fightTimer > 30f && this._secondPhaseTimer <= 20f)
            {
                this._secondPhaseTimer += Time.deltaTime;

                if (Time.time >= this._nextSpawnTimeRoll)
                {
                    SpawnSpecialObject();
                    this._nextSpawnTimeRoll = Time.time + 2f;
                }
            }

            if (this._fightTimer > 60f && this._thirdPhaseTimer <= 20f) {
                
                this._thirdPhaseTimer += Time.deltaTime;
                if (Time.time >= this._nextSpawnTimeFall)
                {
                    SpawnFallingRock();
                    this._nextSpawnTimeFall = Time.time + Random.Range(0.2f, 2f);
                }
                if (Time.time >= this._nextSpawnTimeRoll)
                {
                    SpawnSpecialObject();
                    this._nextSpawnTimeRoll = Time.time + 2f;
                }
            }
        }

        
        void SpawnFallingRock()
        {
            if (objectToSpawn == null) return;

            float distance = Random.Range(minXDistance * 2f, maxXDistance * 2f);
            float spawnX = transform.position.x - distance;
            float spawnY = transform.position.y + spawnHeight;

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            GameObject obj = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(obj);
        }

        void SpawnSpecialObject()
        {
            if (specialObjectToSpawn == null) return;

            float spawnX = transform.position.x - 3f;
            float spawnY = Random.value < 0.5f ? transform.position.y - 8f : transform.position.y- 4f;

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            
            GameObject obj = Instantiate(specialObjectToSpawn, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(obj);
        }

        private void OnPVChanged(float newPv) {
            this.PV = newPv;
        }

        public int Hit(float damageTook) {
            if (!isServer) return 0;
            if (this._vulnerable) {
                this.PV -= damageTook;
                if (this.PV <= 0) {
                    this.Die();
                    return 0;
                }
                else {
                    this._animator.SetTrigger(GetHitted);
                    return 0;
                }
            }

            return 0;
        }

        public void Die() {
            this.isAlive = false;
            this._animator.SetBool(IsAlive, false);
            this.RpcDie();
        }

        [ClientRpc]
        private void RpcDie() {
            if (!this.isActiveAndEnabled) return;
            //son
            this.body.simulated = false;
        }
        
        
    }

}
