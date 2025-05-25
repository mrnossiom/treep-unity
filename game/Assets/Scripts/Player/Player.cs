using System.Collections;
using Mirror;
using TMPro;
using Treep.Weapon;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

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

        [SyncVar(hook = nameof(Player.OnWeaponChanged))]
        public Weapons weapon;

        public int money;
        public float damageMultiplier;
        public float dashMultiplier;
        public float visionMultiplier;

        private WeaponManager _weaponManager;
        private PlayerController _controllerScript;
        private PlayerAnimatorController _animatorController;
        private Rigidbody2D _rigidbody2d;
        private PlayerCombat _playerCombat;

        [SerializeField] private TextMeshPro usernameText;

        private void Awake() {
            this._rigidbody2d = this.GetComponent<Rigidbody2D>();
            this._controllerScript = this.GetComponent<PlayerController>();
            this._weaponManager = this.GetComponent<WeaponManager>();
            this._playerCombat = this.GetComponent<PlayerCombat>();
            this._animatorController = this.GetComponent<PlayerAnimatorController>();
        }

        [Server]
        public void InitStats() {
            this.maxHealth = 10;
            this.health = this.maxHealth;
            this.money = 0;
            this.damageMultiplier = 1f;
            this.weapon = Weapons.Fist;
            this.dashMultiplier = 1f;
        }

        public override void OnStartLocalPlayer() {
            Player.Singleton = this;
            this.maxHealth = 10;
            this.health = this.maxHealth;
            this.money = 0;
            this.damageMultiplier = 1f;
            this.dashMultiplier = 1f;
            this.visionMultiplier = 1f;
            this.weapon = Weapons.Fist;
            this._weaponManager.SwitchWeapon(this.weapon);

            var cinemachineCamera = GameObject.FindWithTag("PlayerVirtualCamera").GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = this.transform;
        }

        public void Update() {
            if (!this.isLocalPlayer) return;
            
            if (Input.GetKeyDown(KeyCode.K)) {
                CmdTakeDamage(50f);
            }
            
            if (Input.GetKeyDown(KeyCode.P)) {
                money += 100;
            }
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

        private void OnWeaponChanged(Weapons oldWeapon, Weapons newWeapon) {
            if (this._weaponManager != null) this._weaponManager.SwitchWeapon(newWeapon);
            if (this._animatorController != null) this._animatorController.SwitchWeapon(newWeapon);
        }

        [Command]
        public void CmdChangeWeapon(Weapons newWeapon) {
            this.weapon = newWeapon;
        }

        public void IncreaseVision() {
            var visionMask = GameObject.FindGameObjectWithTag("PlayerVirtualCamera").transform.Find("VisionMask");
            visionMask.localScale *= this.visionMultiplier;

            var shader = GameObject.FindGameObjectWithTag("PlayerVirtualCamera").transform.Find("ShadowLayer")
                .GetComponent<Renderer>().material;
            shader.SetFloat("_Width", shader.GetFloat("_Width") * this.visionMultiplier);
            shader.SetFloat("_Height", shader.GetFloat("_Height") * this.visionMultiplier);
        }
    }
}
