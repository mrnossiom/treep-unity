using UnityEngine;

namespace Treep.Utils {
    public class SetImguiFont : MonoBehaviour {
        [SerializeField] private Font font;

        private bool _fontInitialized = false;

        void OnGUI() {
            if (!_fontInitialized) {
                GUI.skin.font = font;
                GUI.skin.GetStyle("Toggle").font = font;
                GUI.skin.GetStyle("Label").font = font;

                _fontInitialized = true;
            }
        }
    }
}