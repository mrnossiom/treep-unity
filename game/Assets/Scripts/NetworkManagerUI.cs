using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace KrommProject
{
    public class NetworkManagerUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;

        private void Start()
        {
            // hostButton.onClick(() => { NetworkManager });
        }

        private void Update()
        {
        }
    }
}
