using UnityEngine;

namespace Treep.Utils {
    public class SetImguiFont : MonoBehaviour {
        [SerializeField] private Font font;

        private bool _fontInitialized = false;

        private void OnGUI() {
            if (this._fontInitialized) return;
            this._fontInitialized = true;

            GUI.skin.font = this.font;
            GUI.skin.GetStyle("Toggle").font = this.font;
            GUI.skin.GetStyle("Label").font = this.font;
        }
    }
}
