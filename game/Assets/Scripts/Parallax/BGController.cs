using System.Collections;
using UnityEngine;

namespace Treep {
    public class BackgroundController : MonoBehaviour {
        private Vector2 _startPos;
        private float _length;

        public GameObject cam;
        public float backgroundSpeed; // The intensity of the parallax effect 0 -> 1 (0 move a lot and 1 don't move)

        private void Start() {
            this._startPos = this.transform.position;
            this._length = 64;
        }

        private void FixedUpdate() {
            var distance = this.cam.transform.position.x * this.backgroundSpeed;
            var movement = this.cam.transform.position.x * (1 - this.backgroundSpeed);

            this.transform.position = new Vector3(
                this._startPos.x + distance,
                this._startPos.y + this.cam.transform.position.y,
                this.transform.position.z);


            if (movement > this._startPos.x + this._length) {
                this._startPos.x += this._length;
            }
            else if (movement < this._startPos.x - this._length) {
                this._startPos.x -= this._length;
            }
        }
    }
}
