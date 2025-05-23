using System;
using UnityEngine;

namespace Treep.SFX {
    public class MusicManager : MonoBehaviour {
        private static MusicManager instance;

        private void Awake() {
            if (MusicManager.instance != null) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
