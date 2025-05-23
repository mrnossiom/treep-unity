using UnityEngine;

namespace Treep.IA {
    public class Shop : MonoBehaviour {
        [Header("NPC")]
        public GameObject test;
        
        [Header("Shop UI")]
        public GameObject shopUI;
        
        private bool _playerInRange = false;
        private Player.Player _player;
        
        private void Start() { }

        private void Update() {
            if (this._playerInRange && Input.GetKeyDown(KeyCode.E)) {
                //this.shopUI.SetActive(!this.shopUI.activeSelf);
                Debug.Log(this._player);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player")) {
                this._playerInRange = true;
                this._player = other.gameObject.GetComponent<Player.Player>();
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag("Player")) {
                this._playerInRange = false;
                this.shopUI.SetActive(false);
                this._player = null;
            }
        }
    }
}
