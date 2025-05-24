using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Treep.Interface {
    public class JoinGameMenu : MonoBehaviour {
        [SerializeField] private GameObject joinMenu;
        [SerializeField] private TMP_InputField inputFieldIPPort;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button cancelButton;

        private void Start() {
            if (this.connectButton != null)
                this.connectButton.onClick.AddListener(ConnectToServer);
            if (this.cancelButton != null)
                this.cancelButton.onClick.AddListener(HideJoinMenu);
        }
        
        public void HideJoinMenu() {
            if (this.joinMenu != null)
                this.joinMenu.SetActive(false);
        }
        
        public void ConnectToServer() {
            if (inputFieldIPPort == null) {
                return;
            }

            var input = inputFieldIPPort.text.Trim();

            if (TryParseIPPort(input, out var ip, out var port)) {
                NetworkManager.singleton.networkAddress = ip;

                if (NetworkManager.singleton.transport is KcpTransport transport) {
                    transport.Port = port;
                }
                
                NetworkManager.singleton.StartClient();
                HideJoinMenu();
            }
        }
        
        private bool TryParseIPPort(string input, out string ip, out ushort port) {
            ip = null;
            port = 0;

            var parts = input.Split(':');
            if (parts.Length != 2) {
                return false;
            }

            ip = parts[0];

            return ushort.TryParse(parts[1], out port);
        }
    }
}