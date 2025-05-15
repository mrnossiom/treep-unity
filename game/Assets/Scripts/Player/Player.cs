using System;
using Mirror;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace Treep.Player {
    [RequireComponent(typeof(NetworkIdentity), typeof(Rigidbody2D))]
    public class Player : NetworkBehaviour {
        public static Player Singleton { get; private set; }

        private Rigidbody2D _rigidbody2d;

        [SerializeField] private TextMeshPro usernameText;

        [SyncVar(hook = nameof(OnUsernameChanged))]
        public string username;
        
        [SyncVar]
        public bool isReady = false;

        private void Awake() {
            _rigidbody2d = GetComponent<Rigidbody2D>();
        }

        public override void OnStartLocalPlayer() {
            Singleton = this;

            var cinemachineCamera = GameObject.FindWithTag("PlayerVirtualCamera").GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = transform;
        }

        public void SetSimulated(bool simulated) {
            _rigidbody2d.simulated = simulated;
        }
        
        void OnUsernameChanged(string oldUsername, string newUsername) {
            if (usernameText != null) usernameText.text = newUsername;
        }
        
        [Command]
        public void CmdSetReady(bool ready) {
            isReady = ready;
        }
        
    }
}
