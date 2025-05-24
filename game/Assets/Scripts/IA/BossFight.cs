using System;
using System.Linq;
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
        private bool _isAlive = true;
        public GameObject objectToSpawn;
        public GameObject specialObjectToSpawn;
        public float minXDistance = 3f;
        public float maxXDistance = 10f;
        public float spawnHeight = 10f;
        private bool _isTrigger;
        private Vector2 _triggerZoneSize = new (45f, 30f);
        [SerializeField] private Vector2 _triggerZonePos = new(-15f, 3f) ;
        public LayerMask playerLayerMask;
        
        [SyncVar(hook = nameof(OnPVChanged))]
        private float _pv;

        public int loot = 100;

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

        void FixedUpdate() {
            
            if (!this._isAlive ) return;
            
            if (!this._isTrigger ) {
                UpdateTriggerZone();
                return;
            }
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
            else {
                this._vulnerable = false;
                this._animator.SetBool(Vulnerable, false);
            }
            if (this._fightTimer > 30f && this._secondPhaseTimer <= 20f)
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

        public void UpdateTriggerZone() {
            Player.Player[] players = DetectPlayerinTriggerZone();
            if (players.Length == 0) {
                return;
            }

            OnTriggerPlayerEnter();
            this._isTrigger = true;
        }

        
        public Player.Player[] DetectPlayerinTriggerZone() {
        
            var results = new Collider2D[50];
            var count = 0;

            var filter = new ContactFilter2D();
            filter.SetLayerMask(this.playerLayerMask);
            filter.useTriggers = true;

            
            count = Physics2D.OverlapBox((Vector2)this.transform.position + this._triggerZonePos, this._triggerZoneSize, 0f, filter, results);

            
            return (from collider2d in results.Take(count)
                select collider2d.GetComponent<Player.Player>()).ToArray();
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

        private void OnPVChanged(float odlPV, float newPV) {
            this.PV = newPV;
        }

        public int Hit(float damageTook) {
            
            if (!isServer) return 0;
            
            if (this._vulnerable) {
                Debug.Log("Le boss se fait hit et est vulneable");
                this.PV -= damageTook;
                if (this.PV <= 0) {
                    this.Die();
                    return this.loot;
                }
                else {
                    this._animator.SetTrigger(GetHitted);
                    return 0;
                }
            }
            Debug.Log("Le boss se fait hit pas vulerable");
            return 0;
        }

        public void Die() {
            this._isAlive = false;
            this._animator.SetBool(IsAlive, false);
            this.RpcDie();
        }

        [ClientRpc]
        private void RpcDie() {
            if (!this.isActiveAndEnabled) return;
            //son
            this.body.simulated = false;
        }

        private void OnTriggerPlayerEnter() {
        }


        private void OnDrawGizmosSelected() {
            Debug.Log($"Is Alive : " + this._isAlive +
                      $"\n _vulnerable :{this._vulnerable}");
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube((Vector2)this.transform.position + this._triggerZonePos, this._triggerZoneSize);
        }
    }

}
