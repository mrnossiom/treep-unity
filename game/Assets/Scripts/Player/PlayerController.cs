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
    public sealed class PlayerController : NetworkBehaviour {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int JumpStart = Animator.StringToHash("JumpStart");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int JumpEnd = Animator.StringToHash("JumpEnd");

        // Components
        private Collider2D _collider2d;
        private Rigidbody2D _body;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        
        [SyncVar(hook = nameof(OnSpriteFlip))]
        private bool isFlipped;

        private ContactFilter2D _contactFilter;
        private TagHandle _ladderTag;

        // Config
        private const float MinGroundNormalY = .65f;
        private const float MinMoveDistance = 0.001f;
        private const float ShellRadius = 0.01f;

        [SerializeField] private float gravityModifier = 1.5f;
        [SerializeField] private float maxSpeed = 7;
        [SerializeField] private float jumpTakeOffSpeed = 6;

        [SerializeField] private float jumpModifier = 1.2f;
        [SerializeField] private float jumpDeceleration = 0.5f;

        [SerializeField] private float climbSpeed = 3f;

        [SerializeField] private bool controlEnabled = true;

        // State
        private Vector2 _targetVelocity;
        private Vector2 _groundNormal;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];

        private Vector2 _velocity;

        private JumpState _jumpState = JumpState.Grounded;
        private bool _stopJump;

        private bool _jump;
        private Vector2 _move;

        private bool IsGrounded { get; set; }
        private bool IsClimbing { get; set; }

        private void Awake() {
            _body = GetComponent<Rigidbody2D>();
            _collider2d = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();

            _ladderTag = TagHandle.GetExistingTag("Ladder");

            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        }
        
        private void OnSpriteFlip(bool oldValuve, bool newValue)
        {
            _spriteRenderer.flipX = newValue;
        }
        
        [Command]
        private void CmdSetFlip(bool flip) {
            isFlipped = flip;
        }

        private void Update() {
            if (!isLocalPlayer) return;

            if (controlEnabled) {
                _move.x = Input.GetAxis("Horizontal");
                _animator.SetBool(IsMoving, _move.x != 0);
                
                if (_move.x < 0 != isFlipped) CmdSetFlip(_move.x < 0);

                if (_jumpState == JumpState.Grounded && Input.GetButtonDown("Jump")) {
                    _animator.SetBool(JumpStart, true);
                    _animator.SetBool(IsJumping, false);
                    _animator.SetBool(JumpEnd, false);
                    _jumpState = JumpState.PrepareToJump;
                }

                else if (Input.GetButtonUp("Jump")) _stopJump = true;

                if (IsClimbing) {
                    _move.y = Input.GetAxis("Vertical");
                }
                else {
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
                    break;
                case JumpState.Jumping:
                    _animator.SetBool(JumpStart, false);
                    if (!IsGrounded) _jumpState = JumpState.InFlight;
                    break;
                case JumpState.InFlight:
                    _animator.SetBool(IsJumping, true);
                    if (IsGrounded) _jumpState = JumpState.Landed;
                    break;
                case JumpState.Landed:
                    _jumpState = JumpState.Grounded;
                    _animator.SetBool(JumpEnd, true);
                    break;
            }
        }

        private void ComputeVelocity() {
            if (_jump && IsGrounded) {
                _velocity.y = jumpTakeOffSpeed * jumpModifier;
                _jump = false;
            }
            else if (_stopJump) {
                _stopJump = false;
                if (_velocity.y > 0) _velocity.y = _velocity.y * jumpDeceleration;
            }

            _targetVelocity = _move * maxSpeed;

            if (IsClimbing) {
                _velocity.y = _move.y * climbSpeed;
            }
        }

        private void FixedUpdate() {
            if (IsClimbing) {
                _velocity.y = _move.y * climbSpeed;
            }
            else {
                if (_velocity.y < 0)
                    _velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
                else
                    _velocity += Physics2D.gravity * Time.deltaTime;
            }

            _velocity.x = _targetVelocity.x;
            IsGrounded = false;

            var deltaPosition = _velocity * Time.deltaTime;
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
                        if (_velocity.y > 0)
                            _velocity.y = 0;
                    }

                    if (IsGrounded) {
                        var projection = Vector2.Dot(_velocity, currentNormal);
                        if (projection < 0)
                            _velocity = _velocity - projection * currentNormal;
                    }
                    else {
                        _velocity.x *= 0;
                    }

                    var modifiedDistance = _hitBuffer[i].distance - ShellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }

            _body.position += move.normalized * distance;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag(_ladderTag)) return;

            IsClimbing = true;
            _velocity.y = 0;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag(_ladderTag)) return;

            IsClimbing = false;
            _velocity.y = 0;
        }
    }
}
