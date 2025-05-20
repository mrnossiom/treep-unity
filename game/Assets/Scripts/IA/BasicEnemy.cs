using UnityEngine;
using UnityEngine.UI;

namespace Treep.IA {
    public class BasicEnemy : MonoBehaviour, IEnemy {
        private static readonly int GetHit = Animator.StringToHash("GetHit");

        public int PVMax;

        public Rigidbody2D _body;
        public Collider2D _collider2d;
        public SpriteRenderer _spriteRenderer;
        public Animator _animator;
        public int speed = 3;

        public float detectionDistance = 0.2f;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

        private readonly bool _moveEnabled = true;
        private readonly Vector2 moveDirection = Vector2.right;
        private ContactFilter2D _contactFilter;
        private int _direction = 1;


        public void Awake() {
            this._body = this.GetComponent<Rigidbody2D>();
            this._collider2d = this.GetComponent<Collider2D>();
            this._spriteRenderer = this.GetComponent<SpriteRenderer>();
            this._animator = this.GetComponent<Animator>();
        }

        private void Start() {
            this.PV = this.PVMax;
            this._contactFilter.useLayerMask = true;
            this._contactFilter.layerMask = LayerMask.GetMask("Wall");
        }

        private void FixedUpdate() {
            if (!this._moveEnabled) return;

            this._animator.SetBool(BasicEnemy.GetHit, false);

            if (this.IsWall() && this.PV > 0) {
                this._direction = this._direction * -1;
                this._spriteRenderer.flipX = this._direction < 0;
            }

            this._body.linearVelocity = new Vector2(this._direction * this.speed, this._body.linearVelocity.y);
        }

        public int PV { get; set; }

        public void Hit(int damageTook) {
            this.PV -= damageTook;

            if (this.PV <= 0) {
                this.Die();
            }
            else {
                this._animator.SetBool(BasicEnemy.GetHit, true);
            }
        }

        private bool IsWall() {
            var hits = new RaycastHit2D[16];
            var count = this._body.Cast(this.moveDirection, hits, this.detectionDistance);

            return count > 0;
        }

        public void Die() {
            // Die animation 
            this.GetComponent<Collider2D>().enabled = false;
            this.GetComponent<SpriteRenderer>().enabled = false;
            this.enabled = false;
        }
    }
}
