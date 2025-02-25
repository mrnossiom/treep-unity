using UnityEngine;

namespace Treep.IA {
    public class BasicEnemy : MonoBehaviour, IEnemy {
        public int PVMax;

        public Rigidbody2D _body;
        public Collider2D _collider2d;
        public SpriteRenderer _spriteRenderer;
        public Animator _animator;

        public int PV { get; set; }

        public void Awake() {
            this._body = this.GetComponent<Rigidbody2D>();
            this._collider2d = this.GetComponent<Collider2D>();
            this._spriteRenderer = this.GetComponent<SpriteRenderer>();
            this._animator = this.GetComponent<Animator>();
        }

        private void Start() {
            this.PV = this.PVMax;
        }

        private void Update() { }

        public void GetHitted(int damageTook) {
            Debug.Log($"{this} took {damageTook} damage now Pv = {this.PV - damageTook}");
            this.PV -= damageTook;
            if (this.PV <= 0) {
                this.Die();
            }
            else {
                // hurt animation
            }
        }

        private void Die() {
            Debug.Log(this.ToString() + " Dead");
            //Die animation 
            this.GetComponent<Collider2D>().enabled = false;
            this.GetComponent<SpriteRenderer>().enabled = false;
            this.enabled = false;
        }

        public override string ToString() {
            return $"{this.transform.name}";
        }
    }
}
