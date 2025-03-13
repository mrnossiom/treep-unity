using UnityEngine;
using UnityEngine.UI;

namespace Treep.Utils {
    [RequireComponent(typeof(Image))]
    public class ImageAnimation : MonoBehaviour {
        public Sprite[] sprites;
        public int framesPerSprite = 6;
        public bool loop = true;
        public bool destroyOnEnd = false;

        private int index = 0;
        private Image image;
        private int frame = 0;

        private void Awake() {
            this.image = this.GetComponent<Image>();
        }

        private void FixedUpdate() {
            if (!this.loop && this.index == this.sprites.Length) {
                return;
            }

            this.frame++;
            if (this.frame < this.framesPerSprite) {
                return;
            }

            this.image.sprite = this.sprites[this.index];
            this.frame = 0;
            this.index++;
            if (this.index >= this.sprites.Length) {
                if (this.loop) this.index = 0;
                if (this.destroyOnEnd) {
                    Object.Destroy(this.gameObject);
                }
            }
        }
    }
}
