using System.Collections;
using Mirror;
using TMPro;
using Treep.Weapon;
using Unity.Cinemachine;
using UnityEngine;

namespace Treep.Player {
    [RequireComponent(typeof(NetworkIdentity), typeof(Rigidbody2D))]
    public class Player : NetworkBehaviour {
        public static Player Singleton { get; private set; }

        [SyncVar(hook = nameof(Player.OnUsernameChanged))]
        public string username;

        [SyncVar] public bool isReady = false;
        [SyncVar] public bool isDead = false;

        public int maxHealth;
        [SyncVar] public float health;
        public Weapons weapon;
        public int money;
        public float damageMultiplier;

        private WeaponManager _weaponManager;
        private PlayerController _controllerScript;
        private Rigidbody2D _rigidbody2d;
        private PlayerCombat _playerCombat;

        [SerializeField] private TextMeshPro usernameText;

        private void Awake() {
            this._rigidbody2d = this.GetComponent<Rigidbody2D>();
            this._controllerScript = this.GetComponent<PlayerController>();
            this._weaponManager = this.GetComponent<WeaponManager>();
            this._playerCombat = this.GetComponent<PlayerCombat>();
        }
        
        [Server]
        public void InitStats() {
            this.maxHealth = 10;
            this.health = this.maxHealth;
            this.money = 0;
            this.damageMultiplier = 1f;
            this.weapon = Weapons.Fist;
        }
        
        public override void OnStartLocalPlayer() {
            Player.Singleton = this;
            this.maxHealth = 10;
            this.health = this.maxHealth;
            this.money = 0;
            this.damageMultiplier = 1f;
            this.weapon = Weapons.Fist;
            this._weaponManager.SwitchWeapon(this.weapon);
            var cinemachineCamera = GameObject.FindWithTag("PlayerVirtualCamera").GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = this.transform;
        }

        public void Update() {
            #if UNITY_EDITOR
            if (!this.isLocalPlayer) return;
            if (Input.GetKeyDown(KeyCode.K)) {
                Player.Singleton.CmdTakeDamage(50f);
            }
            #endif
        }

        public void SetSimulated(bool simulated) {
            this._rigidbody2d.simulated = simulated;
        }

        private void OnUsernameChanged(string oldUsername, string newUsername) {
            if (this.usernameText != null) this.usernameText.text = newUsername;
        }

        [Command]
        public void CmdSetReady(bool ready) {
            this.isReady = ready;
        }

        [Command]
        public void CmdTakeDamage(float damage) {
            //if (!this.isLocalPlayer) return;
            this.TakeDamage(damage);
        }

        [Server]
        public void TakeDamage(float damage) {
            if (this.isDead) return;
            this.health -= damage;
            if (this.health <= 0) {
                this.Die();
            }
        }

        [Server]
        private void Die() {
            this.isDead = true;
            this.RpcHandleDeath();
            State.NetworkController.Instance.CheckAllPlayersDead();
        }

        [ClientRpc]
        private void RpcHandleDeath() {
            var renderers = this.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers) {
                var c = sr.color;
                c.a = 0.5f;
                sr.color = c;
            }

            if (this._weaponManager != null) this._weaponManager.enabled = false;
            if (this._playerCombat != null) this._playerCombat.enabled = false;
        }

        [Server]
        public void Respawn() {
            this.isDead = false;
            this.isReady = false;
            this.health = this.maxHealth;
            this.RpcHandleRespawn();
        }

        [ClientRpc]
        private void RpcHandleRespawn() {
            var renderers = this.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers) {
                var c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            if (this._weaponManager != null) this._weaponManager.enabled = true;
            if (this._playerCombat != null) this._playerCombat.enabled = true;
        }
    }
}
