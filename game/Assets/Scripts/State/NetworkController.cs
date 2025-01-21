using Mirror;

namespace Treep.State {
    public class NetworkController : NetworkManager {
        public override void OnStartServer() {
            base.OnStartServer();

            NetworkServer.RegisterHandler<CreateCharacterMessage>(OnCreateCharacter);
        }

        public override void OnClientConnect() {
            base.OnClientConnect();

            var characterMessage = new CreateCharacterMessage {
                Username = Settings.Singleton.username,
                Color = Settings.Singleton.color,
            };
            NetworkClient.Send(characterMessage);
        }

        void OnCreateCharacter(NetworkConnectionToClient conn, CreateCharacterMessage message) {
            var playerObj = Instantiate(playerPrefab);
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
