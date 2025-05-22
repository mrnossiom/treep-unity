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

        [SyncVar(hook = nameof(OnUsernameChanged))] public string username;
        [SyncVar] public bool isReady = false;
        [SyncVar] public bool isDead = false;

        public float maxHealth = 100f;
        public float health = 100f;
        public Weapons weapon;
        public int money;
        public float damageMultiplier = 1f;

        private WeaponManager _weaponManager;
        private PlayerController _controllerScript;
        private Rigidbody2D _rigidbody2d;

        [SerializeField] private TextMeshPro usernameText;

        private void Awake() {
            _rigidbody2d = GetComponent<Rigidbody2D>();
            _controllerScript = GetComponent<PlayerController>();
            _weaponManager = GetComponent<WeaponManager>();
        }

        public override void OnStartLocalPlayer() {
            Singleton = this;
            LoadPlayerData();
            var cinemachineCamera = GameObject.FindWithTag("PlayerVirtualCamera").GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = transform;
        }

        public void Update() {
            if (!isLocalPlayer) return;
            
            if (Input.GetKeyDown(KeyCode.K)) {
                Player.Singleton.CmdTakeDamage(50f);
            }
        }

        public void SetSimulated(bool simulated) {
            _rigidbody2d.simulated = simulated;
        }

        private void OnUsernameChanged(string oldUsername, string newUsername) {
            if (usernameText != null) usernameText.text = newUsername;
        }

        [Command]
        public void CmdSetReady(bool ready) {
            isReady = ready;
        }

        public void SavePlayerData() {
            PlayerPrefs.SetInt("Money", money);
            PlayerPrefs.SetFloat("DamageMultiplier", damageMultiplier);
            PlayerPrefs.SetInt("SelectedWeapon", (int)weapon);
            PlayerPrefs.Save();
        }


        public void LoadPlayerData() {
            money = PlayerPrefs.GetInt("Money", 0);
            damageMultiplier = PlayerPrefs.GetFloat("DamageMultiplier", 1.0f);
            weapon = (Weapons)PlayerPrefs.GetInt("SelectedWeapon", (int)Weapons.Fist);
        }
        
        [Command]
        public void CmdTakeDamage(float damage) {
            TakeDamage(damage);
        }

        [Server]
        public void TakeDamage(float damage) {
            if (isDead) return;
            health -= damage;
            if (health <= 0) {
                Die();
            }
        }

        [Server]
        private void Die() {
            isDead = true;
            SavePlayerData();
            RpcHandleDeath();
            State.NetworkController.Instance.CheckAllPlayersDead();
        }

        [ClientRpc]
        private void RpcHandleDeath() {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers) r.enabled = false;
            
            if (_weaponManager != null) _weaponManager.enabled = false;
        }

        [Server]
        public void Respawn() {
            this.isDead = false;
            this.isReady = false;
            this.health = 100f;
            LoadPlayerData();
            RpcHandleRespawn();
        }

        [ClientRpc]
        private void RpcHandleRespawn() {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers) r.enabled = true;
            if (_weaponManager != null) _weaponManager.enabled = true;
        }
    }
}
