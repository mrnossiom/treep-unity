using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface {
    public class HealthBar : MonoBehaviour {
        public Image fillImage;

        private float _displayedHealth;
        public float animationSpeed = 40f;

        private void Start() {
            this._displayedHealth = Player.Player.Singleton.health;
            this.UpdateFillInstant();
        }

        private void Update() {
            if (Player.Player.Singleton == null) return;

            var targetHealth = Player.Player.Singleton.health;

            if (Mathf.Abs(this._displayedHealth - targetHealth) > 0.01f) {
                this._displayedHealth = Mathf.MoveTowards(this._displayedHealth, targetHealth,
                    this.animationSpeed * Time.deltaTime);
                this.UpdateFill();
            }
        }

        private void UpdateFill() {
            this.fillImage.fillAmount = this._displayedHealth / Player.Player.Singleton.maxHealth;
        }

        private void UpdateFillInstant() {
            this.fillImage.fillAmount = Player.Player.Singleton.health / Player.Player.Singleton.maxHealth;
        }
    }
}
