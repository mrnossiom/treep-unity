using Mirror;
using Treep.State;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Treep.Interface
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button createGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private NetworkController networkControllerPrefab;

        [SerializeField] private Button startServerButton;

        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsMenu settingsPane;

        void Start()
        {
            createGameButton.onClick.AddListener(() =>
            {
                DontDestroyOnLoad(Instantiate(networkControllerPrefab));
                NetworkManager.singleton.StartHost();
            });
            joinGameButton.onClick.AddListener(() =>
            {
                DontDestroyOnLoad(Instantiate(networkControllerPrefab));
                NetworkManager.singleton.StartClient();
            });

            startServerButton.onClick.AddListener(() =>
            {
                DontDestroyOnLoad(Instantiate(networkControllerPrefab));
                NetworkManager.singleton.StartServer();
            });

            settingsButton.onClick.AddListener(() => { settingsPane.gameObject.SetActive(true); });
        }
    }
}
