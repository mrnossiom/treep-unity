using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Treep.Utils {
    public class InitializeOnce : MonoBehaviour {
        [SerializeField] private List<GameObject> objects;

        private static bool _initialized = false;

        private void Awake() {
            if (InitializeOnce._initialized) return;
            InitializeOnce._initialized = true;

            foreach (var target in this.objects.Select(Object.Instantiate)) {
                Object.DontDestroyOnLoad(target);
            }

            Object.Destroy(this.gameObject);
        }
    }
}
