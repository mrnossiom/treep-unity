using UnityEngine;

namespace Treep.IA {
    public class BasicEnemy : MonoBehaviour, IEnemy {
        public int PVMax;

        public Rigidbody2D _body;
        public Collider2D _collider2d;
        public SpriteRenderer _spriteRenderer;
        public Animator _animator;

        public int PV { get; set; }
        public int speed = 3;

        private bool _moveEnabled = true;
        private int _direction = 1;

        public float detectionDistance = 0.2f;
        private Vector2 moveDirection = Vector2.right;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private ContactFilter2D _contactFilter;

        public void Awake() {
            this._body = this.GetComponent<Rigidbody2D>();
            this._collider2d = this.GetComponent<Collider2D>();
            this._spriteRenderer = this.GetComponent<SpriteRenderer>();
            this._animator = this.GetComponent<Animator>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start() {
            this.PV = this.PVMax;
            this.transform.position += new Vector3(5, 5, 0);
            this.transform.name = "Ennemy 1";
            this._contactFilter.useLayerMask = true;
            this._contactFilter.layerMask = LayerMask.GetMask("Wall");
        }

        // Update is called once per frame v
        private void FixedUpdate() {
            if (this._moveEnabled) {
                this._animator.SetBool("GetHit", false);
                if (this.IsWall() && this.PV > 0) {
                    this._direction = this._direction * -1;
                    this._spriteRenderer.flipX = this._direction < 0;
                }

                this._body.linearVelocity = new Vector2(this._direction * this.speed, this._body.linearVelocity.y);
            }
        }

        private bool IsWall() {
            var hits = new RaycastHit2D[16];
            var count = this._body.Cast(this.moveDirection, hits, this.detectionDistance);

            return count > 0;
        }

        public void GetHitted(int damageTook) {
            Debug.Log($"{this.ToString()} took {damageTook} damage now Pv = {this.PV - damageTook}");
            this.PV -= damageTook;
            if (this.PV <= 0) {
                this.Die();
            }
            else {
                this._animator.SetBool("GetHit", true);
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
            return $"name : {this.transform.name}" +
                   $"PV : {this.PV}" +
                   $"Degats : ";
        }
    }
}
