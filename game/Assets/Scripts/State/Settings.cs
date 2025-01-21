using UnityEngine;

namespace Treep.State {
    public class Settings : MonoBehaviour {
        public static Settings Singleton;

        [SerializeField] public string username;
        [SerializeField] public string color;

        private void Awake() {
            if (Singleton is not null) {
                DestroyImmediate(this);
                return;
            }

            Singleton = this;
        }
    }
}
