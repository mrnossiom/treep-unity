using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Treep.Interface
{
    public class KeyRebinder : MonoBehaviour
    {
        [Header("Binding Info")]
        public InputActionReference actionRef;
        [Tooltip("Name of the composite part (e.g. up, down, left, right). Leave empty if not part of a composite.")]
        public string compositePartName;

        [Header("UI")]
        public Button rebindButton;
        public TextMeshProUGUI bindingDisplayName;

        private int bindingIndex = -1;
        private const string PlayerPrefsBindingsKey = "rebinds";

        private void Start()
        {
            LoadRebindings();
            FindBindingIndex();
            UpdateUI();
            rebindButton.onClick.AddListener(StartRebinding);
        }

        private void FindBindingIndex()
        {
            bindingIndex = -1;
            var bindings = actionRef.action.bindings;

            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];

                if (!string.IsNullOrEmpty(compositePartName))
                {
                    if (binding.isPartOfComposite && binding.name == compositePartName)
                    {
                        bindingIndex = i;
                        break;
                    }
                }
                else if (!binding.isPartOfComposite && !binding.isComposite)
                {
                    bindingIndex = i;
                    break;
                }
            }

            if (bindingIndex == -1)
            {
                Debug.LogError($"[KeyRebinder] Could not find binding for '{compositePartName}' in action '{actionRef.name}'");
            }
        }

        private void StartRebinding()
        {
            if (bindingIndex == -1)
            {
                Debug.LogError("[KeyRebinder] Invalid binding index.");
                return;
            }

            rebindButton.interactable = false;
            bindingDisplayName.text = "...";
            
            actionRef.action.Disable();
            actionRef.action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .WithExpectedControlType("Key")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    actionRef.action.Enable();
                    rebindButton.interactable = true;
                    UpdateUI();
                    SaveRebindings();
                })
                .Start();
        }

        private void UpdateUI()
        {
            if (bindingIndex == -1) return;

            var binding = actionRef.action.bindings[bindingIndex];
            bindingDisplayName.text = InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }
        
        public void SaveRebindings()
        {
            var actionAsset = actionRef.asset;
            string rebinds = actionAsset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PlayerPrefsBindingsKey, rebinds);
            PlayerPrefs.Save();
        }
        
        public void LoadRebindings()
        {
            var actionAsset = actionRef.asset;
            if (PlayerPrefs.HasKey(PlayerPrefsBindingsKey))
            {
                string rebinds = PlayerPrefs.GetString(PlayerPrefsBindingsKey);
                actionAsset.LoadBindingOverridesFromJson(rebinds);
            }
        }

    }
}
