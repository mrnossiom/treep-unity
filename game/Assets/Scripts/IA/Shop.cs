using UnityEngine;

namespace Treep.IA {
    public class Shop : MonoBehaviour {
        [SerializeField] private ShopMenu shopMenu;
        private Player.Player _player;

        private void Start() {
            if (this.shopMenu == null) {
                var allMenus = Resources.FindObjectsOfTypeAll<ShopMenu>();
                if (allMenus.Length > 0) {
                    this.shopMenu = allMenus[0];
                }
            }
        }

        private void Update() {
            if (this._player != null && Input.GetKeyDown(KeyCode.E)) {
                this.shopMenu.SetActive(!this.shopMenu.isActive);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                this._player = other.GetComponent<Player.Player>();
                this.shopMenu.SetPlayer(this._player);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                this._player = null;
                this.shopMenu.SetActive(false);
                this.shopMenu.SetPlayer(null);
            }
        }
    }
}
