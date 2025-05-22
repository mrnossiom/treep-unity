using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Treep.Interface {
    public class HealthBar : MonoBehaviour {
        public Image fillImage;
        public Player.Player localPlayer;

        private float displayedHealth;
        public float animationSpeed = 40f;

        private void Start() {
            this.localPlayer = Object.FindObjectsByType<Player.Player>(FindObjectsSortMode.None)
                .First(player => player.isLocalPlayer);
            this.displayedHealth = this.localPlayer.health;
            this.UpdateFillInstant();
        }

        private void Update() {
            var targetHealth = this.localPlayer.health;

            if (Mathf.Abs(this.displayedHealth - targetHealth) > 0.01f) {
                this.displayedHealth = Mathf.MoveTowards(this.displayedHealth, targetHealth,
                    this.animationSpeed * Time.deltaTime);
                this.UpdateFill();
            }
        }

        private void UpdateFill() {
            this.fillImage.fillAmount = this.displayedHealth / this.localPlayer.maxHealth;
        }

        private void UpdateFillInstant() {
            this.fillImage.fillAmount = this.localPlayer.health / this.localPlayer.maxHealth;
        }
    }
}
