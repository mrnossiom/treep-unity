using Mirror;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

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
        private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

        // Components
        private Rigidbody2D _body;
        private BoxCollider2D _collider2d;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        
        [SyncVar(hook = nameof(OnSpriteFlip))]
        private bool _isFlipped;

        private ContactFilter2D _contactFilter;
        private TagHandle _ladderTag;

        // Config
        private const float MinGroundNormalY = .65f;
        private const float MinMoveDistance = 0.001f;
        private const float ShellRadius = 0.01f;

        [SerializeField] private float gravityModifier = 1f;
        [SerializeField] private float maxSpeed = 7;
        [SerializeField] private float jumpTakeOffSpeed = 6;

        [SerializeField] private float jumpModifier = 1.2f;
        [SerializeField] private float jumpDeceleration = 0.5f;

        [SerializeField] private float climbSpeed = 3f;

        // State
        private Vector2 _targetVelocity;
        private Vector2 _groundNormal;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];


        private JumpState _jumpState = JumpState.Grounded;
        private bool _stopJump;
        private bool _controlEnabled = true;

        private bool _jump;
        private Vector2 _move;
        public bool IsGrounded { get; set; }

        private float _maxSpeed = 7;


        private Vector2 _standSize = new Vector2(1.4f, 3.2f);
        private Vector2 _crouchSize = new Vector2(1.4f, 2.3f);
        private Vector2 _standOffSet = new Vector2(-0.18f, -0.126588f);
        private Vector2 _crouchOffSet = new Vector2(-0.18f, -0.5325f);
        private bool _unCrouch;
        
        private bool _isDashing;
        private bool _dashAvailable;
        private float _dashSpeed;
        private float _dashDuration = 0.2f;
        private Vector2 _dashDirection;
        private float _dashTime;
        private Vector2 _velocity;

        private bool IsClimbing { get; set; }

        private void Awake() {
            _body = GetComponent<Rigidbody2D>();
            _collider2d = GetComponent<BoxCollider2D>();
            _collider2d.size = _standSize;
            _collider2d.offset = _standOffSet;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();

            _ladderTag = TagHandle.GetExistingTag("Ladder");

            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        }
        
        private void OnSpriteFlip(bool _, bool newValue)
        {
            _spriteRenderer.flipX = newValue;
        }
        
        [Command]
        private void CmdSetFlip(bool flip) {
            this._isFlipped = flip;
        }

        private void Update() {
            if (!isLocalPlayer) return;

            if (_controlEnabled) {
                _move.x = Input.GetAxis("Horizontal");
                _animator.SetBool(IsMoving, _move.x != 0);
                
                if (_move.x < 0 != this._isFlipped) CmdSetFlip(_move.x < 0);

                if (Input.GetKeyDown(KeyCode.C)) {
                    _collider2d.size = _crouchSize;
                    _collider2d.offset = _crouchOffSet;
                    _animator.SetBool(IsCrouching ,true);
                }
                
                if (Input.GetKeyUp(KeyCode.C)) {
                    _unCrouch = true;
                    
                }

                if (_unCrouch && CanStandUp(1.2f)) {
                    _collider2d.size = _standSize;
                    _collider2d.offset = _standOffSet;
                    _animator.SetBool(PlayerController.IsCrouching ,false);
                    _unCrouch = false;
                }

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

                if (IsGrounded)
                {
                    _dashAvailable = true;
                }
                if (Input.GetKeyDown(KeyCode.LeftShift) && !_isDashing && _dashAvailable) {
                    StartDash();
                }
            }
            else {
                _move.x = 0;
                _move.y = 0;
            }
            if (_isDashing) {
                HandleDash();
            }
            
            UpdateJumpState();
            _targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        

        private void StartDash()
        {
            _isDashing = true;
            _dashAvailable = false;
    
            if (_move.x != 0)
            {
                _dashDirection = new Vector2(_move.x, 0).normalized;
            }
            else
            {
                _dashDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
            }
    
            _dashSpeed = 8f;
            _dashTime = _dashDuration;
        }

        private void HandleDash()
        {
            if (_dashTime > 0)
            {
                int obstacleCount = _body.Cast(_dashDirection, _contactFilter, _hitBuffer, 0.3f);
        
                if (obstacleCount > 0)
                {
                    _isDashing = false;
                    _dashTime = 0;
                    _body.linearVelocity = Vector2.zero;
                    _velocity = Vector2.zero;
                    return;
                }
        
                _body.linearVelocity = _dashDirection * _dashSpeed;
                _dashTime -= Time.deltaTime;
            }
            else
            {
                _isDashing = false;
                _body.linearVelocity = Vector2.zero;
                _velocity = Vector2.zero;
            }
        }
        
        private bool CanStandUp(float checkHeight) {
            Vector2 direction = Vector2.up;
            Vector2 topLeft = new Vector2((float)-0.2, 1).normalized;
            Vector2 topRight = new Vector2((float)0.2, 1).normalized;
            int hitCount = _body.Cast(direction, _contactFilter, _hitBuffer, checkHeight);
            hitCount += _body.Cast(topLeft, _contactFilter, _hitBuffer, checkHeight);
            hitCount += _body.Cast(topRight, _contactFilter, _hitBuffer, checkHeight);
            return hitCount == 0;
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
                if (_velocity.y > 0) {
                    _velocity.y *= jumpDeceleration;
                }
            }

            _targetVelocity = _move * maxSpeed;

            if (IsClimbing) {
                _velocity.y = _move.y * climbSpeed;
            }

            _velocity.x = Mathf.Clamp(_velocity.x, -_maxSpeed, _maxSpeed);
            _velocity.y = Mathf.Clamp(_velocity.y, -8f, jumpTakeOffSpeed);
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

                        if (_velocity.y < 0) {
                            _velocity.y = 0;
                        }
                    }

                    if (currentNormal.y < -MinGroundNormalY && _velocity.y > 0) {
                        _velocity.y = 0;
                    }

                    if (IsGrounded) {
                        var projection = Vector2.Dot(_velocity, currentNormal);
                        if (projection < 0)
                            _velocity -= projection * currentNormal;
                    }
                    else {
                        _velocity.x *= 0;
                    }

                    var modifiedDistance = _hitBuffer[i].distance - ShellRadius;
                    distance = Mathf.Min(distance, modifiedDistance);
                }
            }

            _body.position += move.normalized * distance;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ladder")) {
                Vector2 contactPoint = other.ClosestPoint(transform.position);
                if ((contactPoint.x - (int)contactPoint.x  < 0.5 && contactPoint.x - (int)contactPoint.x > 0.02 )|| contactPoint.x - (int)contactPoint.x > 0.98) {
                    this._spriteRenderer.flipX = true;
                }
                else {
                    this._spriteRenderer.flipX = false;
                }
                
                IsClimbing = true;
                _velocity.y = 0;
            }
        }


        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag(_ladderTag)) return;

            IsClimbing = false;
            _velocity.y = 0;
        }



    }
}
