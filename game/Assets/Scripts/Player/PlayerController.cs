using Unity.Netcode;
using UnityEngine;

namespace Treep.Player {
    public enum JumpState {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Rigidbody2D))]
    public sealed class Controller : NetworkBehaviour {
        // Components
        private Rigidbody2D _body;
        private Collider2D _collider2d;

        // Class values
        [SerializeField] private Vector2 velocity;

        private bool IsGrounded { get; set; }

        // Config
        [SerializeField] private float gravityModifier = 1.5f;

        private Vector2 _targetVelocity;
        private Vector2 _groundNormal;
        private ContactFilter2D _contactFilter;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

        private const float MinGroundNormalY = .65f;
        private const float MinMoveDistance = 0.001f;
        private const float ShellRadius = 0.01f;


        // --- second copy
        private JumpState _jumpState = JumpState.Grounded;
        private bool _stopJump;
        private bool _controlEnabled = true;

        private bool _jump;
        private Vector2 _move;

        private float _maxSpeed = 7;
        private float _jumpTakeOffSpeed = 7;

        private float _jumpModifier = 1.5f;
        private float _jumpDeceleration = 0.5f;

        public override void OnNetworkSpawn() {
            if (!IsOwner) Destroy(this);

            name = "Hello";
        }

        private void Awake() {
            _body = GetComponent<Rigidbody2D>();
            _collider2d = GetComponent<Collider2D>();
        }

        private void Update() {
            if (_controlEnabled) {
                _move.x = Input.GetAxis("Horizontal");
                if (_jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    _jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump")) _stopJump = true;
                // Schedule<PlayerStopJump>().player = this;
            }
            else {
                _move.x = 0;
            }

            UpdateJumpState();

            _targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        private void UpdateJumpState() {
            _jump = false;
            switch (_jumpState) {
                case JumpState.PrepareToJump:
                    _jumpState = JumpState.Jumping;
                    _jump = true;
                    _stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                        // Schedule<PlayerJumped>().player = this;
                        _jumpState = JumpState.InFlight;

                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                        // Schedule<PlayerLanded>().player = this;
                        _jumpState = JumpState.Landed;

                    break;
                case JumpState.Landed:
                    _jumpState = JumpState.Grounded;
                    break;
            }
        }

        private void ComputeVelocity() {
            if (_jump && IsGrounded) {
                velocity.y = _jumpTakeOffSpeed * _jumpModifier;
                _jump = false;
            }
            else if (_stopJump) {
                _stopJump = false;
                if (velocity.y > 0) velocity.y = velocity.y * _jumpDeceleration;
            }

            // if (_move.x > 0.01f) spriteRenderer.flipX = false;
            // else if (_move.x < -0.01f) spriteRenderer.flipX = true;

            // animator.SetBool("grounded", IsGrounded);
            // animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            _targetVelocity = _move * _maxSpeed;
        }


        private void FixedUpdate() {
            //if already falling, fall faster than the jump speed, otherwise use normal gravity.
            if (velocity.y < 0)
                velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
            else
                velocity += Physics2D.gravity * Time.deltaTime;

            velocity.x = _targetVelocity.x;

            IsGrounded = false;

            var deltaPosition = velocity * Time.deltaTime;

            var moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);

            var move = moveAlongGround * deltaPosition.x;

            PerformMovement(move, false);

            move = Vector2.up * deltaPosition.y;

            PerformMovement(move, true);
        }


        private void PerformMovement(Vector2 move, bool yMovement) {
            var distance = move.magnitude;

            if (distance > MinMoveDistance) {
                //check if we hit anything in current direction of travel
                var count = _body.Cast(move, _contactFilter, _hitBuffer, distance + ShellRadius);
                for (var i = 0; i < count; i++) {
                    var currentNormal = _hitBuffer[i].normal;

                    //is this surface flat enough to land on?
                    if (currentNormal.y > MinGroundNormalY) {
                        IsGrounded = true;
                        // if moving up, change the groundNormal to new surface normal.
                        if (yMovement) {
                            _groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }

                    if (IsGrounded) {
                        //how much of our velocity aligns with surface normal?
                        var projection = Vector2.Dot(velocity, currentNormal);
                        if (projection < 0)
                            //slower velocity if moving against the normal (up a hill).
                            velocity = velocity - projection * currentNormal;
                    }
                    else {
                        //We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                        velocity.x *= 0;
                        velocity.y = Mathf.Min(velocity.y, 0);
                    }

                    //remove shellDistance from actual move distance.
                    var modifiedDistance = _hitBuffer[i].distance - ShellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }

            _body.position = _body.position + move.normalized * distance;
        }
    }
}
