using Mirror;
using UnityEngine;

namespace Treep.Player {
    public enum JumpState {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Controller : NetworkBehaviour {
        // Components
        private Rigidbody2D _body;
        private Collider2D _collider2d;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        // Class values
        [SerializeField] private Vector2 velocity;

        private bool IsGrounded { get; set; }
        private bool IsClimbing { get; set; }

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
        private float _jumpTakeOffSpeed = 6;

        private float _jumpModifier = 1.2f;
        private float _jumpDeceleration = 0.5f;
        
        private float _climbSpeed = 3f;

        private void Awake() {
            _body = GetComponent<Rigidbody2D>();
            _collider2d = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        private void Update() {
            if (!isLocalPlayer) return;

            if (_controlEnabled) {
                _move.x = Input.GetAxis("Horizontal");
                _animator.SetBool("IsMoving" ,_move.x != 0);
                if (_move.x > 0)
                {
                    _spriteRenderer.flipX = false;
                }
                if (_move.x < 0)
                {
                    _spriteRenderer.flipX = true;
                }
                if (_jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    _jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump")) _stopJump = true;
                if (IsClimbing) {
                    _move.y = Input.GetAxis("Vertical"); 
                } else {
                    _move.y = 0;
                }
            }
            else {
                _move.x = 0;
                _move.y = 0;
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
                    _animator.SetBool("JumpStart" ,true);
                    _animator.SetBool("JumpEnd" ,false);
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        _jumpState = JumpState.InFlight;
                        _animator.SetBool("IsJumping" ,true);
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                        _jumpState = JumpState.Landed;

                    break;
                case JumpState.Landed:
                    _jumpState = JumpState.Grounded;
                    _animator.SetBool("JumpEnd" ,true);
                    _animator.SetBool("IsJumping" ,false);
                    _animator.SetBool("JumpStart" ,false);
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

            _targetVelocity = _move * _maxSpeed;

            if (IsClimbing) {
                velocity.y = _move.y * _climbSpeed; 
            }
        }

        private void FixedUpdate() {
            if (IsClimbing) {
                velocity.y = _move.y * _climbSpeed;
            } 
            else {
                if (velocity.y < 0)
                    velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
                else
                    velocity += Physics2D.gravity * Time.deltaTime;
            }
            velocity.x = _targetVelocity.x;
            IsGrounded = false;
            var deltaPosition = velocity * Time.deltaTime;
            var moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
            var move = moveAlongGround * deltaPosition.x;
            PerformMovement(move);
            move = Vector2.up * deltaPosition.y;
            PerformMovement(move);
        }

        private void PerformMovement(Vector2 move) {
            var distance = move.magnitude;

            if (distance > MinMoveDistance) {
                var count = _body.Cast(move, _contactFilter, _hitBuffer, distance + ShellRadius);
                for (var i = 0; i < count; i++) {
                    var currentNormal = _hitBuffer[i].normal;

                    if (currentNormal.y > MinGroundNormalY) {
                        IsGrounded = true;
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                        
                    }

                    if (currentNormal.y < -MinGroundNormalY) {
                        if (velocity.y > 0)
                            velocity.y = 0;
                    }

                    if (IsGrounded) {
                        var projection = Vector2.Dot(velocity, currentNormal);
                        if (projection < 0)
                            velocity = velocity - projection * currentNormal;
                    }
                    else {
                        velocity.x *= 0;
                    }
                    var modifiedDistance = _hitBuffer[i].distance - ShellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            _body.position += move.normalized * distance;
        }
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Ladder")) {
                IsClimbing = true;
                velocity.y = 0;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            IsClimbing = false;
            velocity.y = 0;
        }
    }
}
