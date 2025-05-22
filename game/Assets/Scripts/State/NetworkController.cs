using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Treep.State {
    public class NetworkController : NetworkManager {
        public static NetworkController Instance { get; private set; }

        [SerializeField] public GameStateManager gameStateManager;
        [SerializeField] private Transform[] spawnPoints;

        private bool _hasGameStarted;

        public override void Awake() {
            base.Awake();
            Instance = this;
        }

        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);
        }

        public override void OnClientConnect() {
            base.OnClientConnect();
            var characterMessage = new CreateCharacterMessage {
                Username = Settings.Singleton.username,
            };
            NetworkClient.Send(characterMessage);
        }

        private void OnCreateCharacter(NetworkConnectionToClient conn, CreateCharacterMessage message) {
            var playerObj = Object.Instantiate(playerPrefab);
            var player = playerObj.GetComponent<Player.Player>();

            player.SetSimulated(false);
            player.username = message.Username;
            player.InitStats();

            NetworkServer.AddPlayerForConnection(conn, playerObj);
            playerObj.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        }

        private void CheckAllPlayersReady() {
            var players = FindObjectsByType<Player.Player>(FindObjectsSortMode.None);
            if (players.All(p => p.isReady) && players.Length > 0) {
                StartGame();
            }
        }

        void StartGame() {
            _hasGameStarted = true;
            gameStateManager.TriggerState(GameStateKind.Level);
        }

        public override void Update() {
            if (this.gameStateManager is null) gameStateManager = FindAnyObjectByType<GameStateManager>();
            if (!_hasGameStarted) CheckAllPlayersReady();
        }

        [Server]
        public void CheckAllPlayersDead() {
            var players = FindObjectsByType<Player.Player>(FindObjectsSortMode.None);
            if (players.Length == 0) return;

            bool allDead = players.All(p => p.isDead);
            if (allDead) {
                Debug.Log("All players are dead");
                RpcReturnToLobby();
            }
        }
        
        private void RpcReturnToLobby() {
            gameStateManager.TriggerState(GameStateKind.Lobby);
            this._hasGameStarted = false;
            var players = FindObjectsByType<Player.Player>(FindObjectsSortMode.None);

            foreach (var player in players) {
                Debug.Log(player.username);
                RespawnPlayer(player);
            }
        }

        [Server]
        public void RespawnPlayer(Player.Player player) {
            player.Respawn();
        }
    }

    public struct CreateCharacterMessage : NetworkMessage {
        public string Username;
    }
}