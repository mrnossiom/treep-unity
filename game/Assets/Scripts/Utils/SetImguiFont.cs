using UnityEngine;

namespace Treep.Utils {
    public class SetImguiFont : MonoBehaviour {
        [SerializeField] private Font font;

        private bool _fontInitialized = false;

        private void OnGUI() {
            if (_fontInitialized) return;
            _fontInitialized = true;

            GUI.skin.font = font;
            GUI.skin.GetStyle("Toggle").font = font;
            GUI.skin.GetStyle("Label").font = font;
        }
    }
}
