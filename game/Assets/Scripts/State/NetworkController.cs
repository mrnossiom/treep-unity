using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Treep.State {
    public class NetworkController : NetworkManager {
        [SerializeField] public GameStateManager gameStateManager;

        private bool _hasGameStarted;
        
        public override void OnStartServer() {
            base.OnStartServer();

            NetworkServer.RegisterHandler<CreateCharacterMessage>(this.OnCreateCharacter);
        }

        public override void OnClientConnect() {
            base.OnClientConnect();

            var characterMessage = new CreateCharacterMessage {
                Username = Settings.Singleton.username,
                Color = Settings.Singleton.color
            };
            NetworkClient.Send(characterMessage);
        }

        private void OnCreateCharacter(NetworkConnectionToClient conn, CreateCharacterMessage message) {
            var playerObj =  Object.Instantiate(this.playerPrefab);
            var player = playerObj.GetComponent<Player.Player>();

            player.SetSimulated(false);
            player.username = message.Username;
            player.color = message.Color;

            NetworkServer.AddPlayerForConnection(conn, playerObj);
            playerObj.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        }
        
        private void CheckAllPlayersReady() {
            var players = FindObjectsByType<Player.Player>(FindObjectsSortMode.None);
            if (players.All(player => player.isReady) && players.Length != 0) StartGame();
        }
        
        void StartGame() {
            this._hasGameStarted = true;
            gameStateManager.TriggerState(GameStateKind.Level);
        }

        public override void Update() {
            gameStateManager = FindAnyObjectByType<GameStateManager>();
            
            if (!this._hasGameStarted) CheckAllPlayersReady();
        }
    }

    public struct CreateCharacterMessage : NetworkMessage {
        public string Username;
        public string Color;
    }
}
