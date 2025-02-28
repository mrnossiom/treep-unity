using Mirror;
using UnityEngine;

namespace Treep.State {
    public class NetworkController : NetworkManager {
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
            var playerObj = Object.Instantiate(this.playerPrefab);
            var player = playerObj.GetComponent<Player.Player>();

            player.SetSimultated(false);
            player.Username = message.Username;
            player.Color = message.Color;

            NetworkServer.AddPlayerForConnection(conn, playerObj);
            playerObj.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        }
    }

    public struct CreateCharacterMessage : NetworkMessage {
        public string Username;
        public string Color;
    }
}
