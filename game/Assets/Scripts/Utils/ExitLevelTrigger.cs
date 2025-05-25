using System;
using Mirror;
using Treep.Level;
using UnityEngine;

namespace Treep.Utils {
    public class ExitLevelTrigger : NetworkBehaviour {
        public LevelAssembler.ExitCallback cb;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.transform.CompareTag("Player")) {
                Debug.Log("exit level");
                this.cb();
            }
        }
    }
}
