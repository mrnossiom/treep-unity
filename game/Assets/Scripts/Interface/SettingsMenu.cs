using TMPro;
using Treep.State;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface {
    public class SettingsMenu : MonoBehaviour {
        [SerializeField] private Button closeButton;

        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField colorInput;

        private void Start() {
            closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });

            usernameInput.text = Settings.Singleton.username;
            usernameInput.onEndEdit.AddListener(username => { Settings.Singleton.username = username; });

            // colorInput.text = Settings.Singleton.color;
            // colorInput.onEndEdit.AddListener(color => { Settings.Singleton.color = color; });
        }
    }
}
