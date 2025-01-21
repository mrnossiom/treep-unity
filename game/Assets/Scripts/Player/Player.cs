using Mirror;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace Treep.Player {
    [RequireComponent(typeof(NetworkIdentity), typeof(Rigidbody2D))]
    public class Player : NetworkBehaviour {
        public static Player Singleton { get; private set; }

        private Rigidbody2D _rigidbody2d;

        [SerializeField] private TextMeshPro username;

        public string Username {
            get => username.text;
            set => username.text = value;
        }

        public string Color { get; set; }

        private void Awake() {
            _rigidbody2d = GetComponent<Rigidbody2D>();
        }

        public override void OnStartLocalPlayer() {
            Singleton = this;

            var cinemachineCamera = GameObject.FindWithTag("PlayerVirtualCamera").GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = transform;
        }

        public void SetSimultated(bool simulated) {
            _rigidbody2d.simulated = simulated;
        }
    }
}
