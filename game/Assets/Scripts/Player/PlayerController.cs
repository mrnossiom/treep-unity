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

    public enum Looking {
        Top,
        Right,
        Bottom,
        Left
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerController : NetworkBehaviour {
        private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int AnimJumpStart = Animator.StringToHash("JumpStart");
        private static readonly int AnimIsJumping = Animator.StringToHash("IsJumping");
        private static readonly int AnimJumpEnd = Animator.StringToHash("JumpEnd");
        private static readonly int AnimIsCrouching = Animator.StringToHash("IsCrouching");
        private static readonly int AnimIsClimbing = Animator.StringToHash("IsClimbing");
        private static readonly int AnimClimbSpeed = Animator.StringToHash("ClimbSpeed");
        private static readonly int AnimIsDashing = Animator.StringToHash("IsDashing");

        // Components
        private Rigidbody2D _body;
        private BoxCollider2D _collider2d;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        [SyncVar(hook = nameof(PlayerController.OnSpriteFlip))]
        private bool _isFlipped;

        private ContactFilter2D _contactFilter;
        private TagHandle _ladderTag;

        // Config
        private const float MinGroundNormalY = .65f;
        private const float MinMoveDistance = 0.001f;
        private const float ShellRadius = 0.01f;

        [SerializeField] private float gravityModifier = 2f;
        [SerializeField] private float maxSpeed = 7;
        [SerializeField] private float jumpTakeOffSpeed = 6;

        [SerializeField] private float jumpModifier = 1.2f;
        [SerializeField] private float jumpDeceleration = 1f;

        [SerializeField] private float climbSpeed = 3f;

        // State
        private Vector2 _targetVelocity;
        private Vector2 _groundNormal;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];


        private JumpState _jumpState = JumpState.Grounded;
        private bool _stopJump;
        private bool _controlEnabled = true;

        private bool _jump;
        public Vector2 _move;
        public bool IsGrounded { get; set; }

        private float _maxSpeed = 7;


        private Vector2 _standSize = new(1.4f, 3.2f);
        private Vector2 _crouchSize = new(1.4f, 2.3f);
        private Vector2 _standOffSet = new(-0.18f, -0.126588f);
        private Vector2 _crouchOffSet = new(-0.18f, -0.5325f);
        private bool _unCrouch;

        private bool _isDashing;
        private bool _dashAvailable;
        private float _dashSpeed;
        private float _dashDuration = 0.2f;
        private Vector2 _dashDirection;
        private float _dashTime;
        private Vector2 _velocity;

        public Transform _dashEffectPoint; //engros ca c'est en fonction de l'endroit du player
        public Vector2 dashEffectPos; // et ca c'est overall
        private Vector2 _currentDashEffectPos; // et ca c'est overall

        private Animator _dashAnimator;
        private SpriteRenderer _dashSpriteRenderer;
        private SpriteRenderer _closeAttackRenderer;
        public PlayerCombat scriptPlayerCombat;

        private bool IsClimbing { get; set; }

        public Looking looking;

        private bool _allSpriteFlipX;

        private bool AllSpriteFlipX {
            get => this._allSpriteFlipX;
            set {
                this._spriteRenderer.flipX = value;
                this._dashSpriteRenderer.flipX = value;
                this._closeAttackRenderer.flipX = value;
                this._allSpriteFlipX = value;
            }
        }


        private void Start() {
            this.transform.position += new Vector3(3, 5, 0);
        }

        private void Awake() {
            this._body = this.GetComponent<Rigidbody2D>();

            this._collider2d = this.GetComponent<BoxCollider2D>();
            this._collider2d.size = this._standSize;
            this._collider2d.offset = this._standOffSet;
            this._spriteRenderer = this.GetComponent<SpriteRenderer>();
            this._animator = this.GetComponent<Animator>();

            this._ladderTag = TagHandle.GetExistingTag("Ladder");

            this._contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(this.gameObject.layer));
            this._dashAnimator = this._dashEffectPoint.GetComponent<Animator>();
            this._dashSpriteRenderer = this._dashEffectPoint.GetComponent<SpriteRenderer>();
            this._closeAttackRenderer = this.scriptPlayerCombat.attackPoint.GetComponent<SpriteRenderer>();
        }

        private void OnSpriteFlip(bool _, bool newValue) {
            this.AllSpriteFlipX = newValue;
        }

        [Command]
        private void CmdSetFlip(bool flip) {
            this._isFlipped = flip;
        }

        private void Update() {
            if (!this.isLocalPlayer) return;

            if (this._controlEnabled) {
                this.UpdateLooking();
                this.UpdateCrouch();
                this.UpdateJump();
                this.UpdateClimb();
                this.UpdateDash();
            }
            else {
                this._move.x = 0;
                this._move.y = 0;
            }

            this._targetVelocity = Vector2.zero;
            this.ComputeVelocity();
        }

        private void UpdateDash() {
            if (this.IsGrounded) {
                this._dashAvailable = true;
            }

            else if (this._move.x < 0) {
                this.AllSpriteFlipX = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && !this._isDashing && this._dashAvailable) {
                this._animator.SetTrigger(PlayerController.AnimIsDashing);
                this._currentDashEffectPos = this.AllSpriteFlipX
                    ? new Vector3(this.transform.position.x + this.dashEffectPos.x,
                        this.transform.position.y + this.dashEffectPos.y)
                    : new Vector3(this.transform.position.x - this.dashEffectPos.x,
                        this.transform.position.y + this.dashEffectPos.y);

                this._dashAnimator.SetTrigger("Dash");
                this.StartDash();
            }

            if (this._isDashing) {
                this.HandleDash();
            }

            this._dashEffectPoint.position
                = new Vector3(this._currentDashEffectPos.x, this._currentDashEffectPos.y);
        }

        private void UpdateClimb() {
            this._animator.SetBool(PlayerController.AnimIsClimbing, this.IsClimbing);

            this._animator.SetFloat(PlayerController.AnimClimbSpeed, this._move.y);
            /*
            if (!this.IsClimbing) {
                this._move.y = Input.GetAxis("Vertical");
            }
            else {
                this._move.y = 0;
            }
            */
        }

        private void UpdateJump() {
            if (this._jumpState == JumpState.Grounded && Input.GetButtonDown("Jump")) {
                this._animator.SetBool(PlayerController.AnimJumpStart, true);
                this._animator.SetBool(PlayerController.AnimIsJumping, false);
                this._animator.SetBool(PlayerController.AnimJumpEnd, false);
                this._jumpState = JumpState.PrepareToJump;
            }
            else if (Input.GetButtonUp("Jump")) this._stopJump = true;

            this.UpdateJumpState();
        }

        private void UpdateLooking() {
            this._move.x = Input.GetAxis("Horizontal");
            this._move.y = Input.GetAxis("Vertical");
            this._animator.SetBool("IsMoving", this._move.x != 0);

            if (this._move.y != 0) // si on cible le haut ou le bas 
            {
                if (this._move.y > 0) // cible top
                {
                    this.looking = Looking.Top;
                }
                else if (this._move.y < 0) // cible bottom
                {
                    this.looking = Looking.Bottom;
                }

                if (this._move.x != 0) {
                    this.AllSpriteFlipX = this._move.x < 0;
                }
            }
            else // le player vas que a gauche ou a droite (ou rien)
            {
                if (this._move.x > 0) {
                    this.AllSpriteFlipX = false;
                }
                else if (this._move.x < 0) {
                    this.AllSpriteFlipX = true;
                }

                this.looking = this.AllSpriteFlipX ? Looking.Left : Looking.Right;
            }
        }

        private void UpdateCrouch() {
            if (Input.GetKeyDown(KeyCode.C)) {
                this._collider2d.size = this._crouchSize;
                this._collider2d.offset = this._crouchOffSet;
                this._animator.SetBool(PlayerController.AnimIsCrouching, true);
            }

            if (Input.GetKeyUp(KeyCode.C)) {
                this._unCrouch = true;
            }

            if (this._unCrouch && this.CanStandUp(1.2f)) {
                this._collider2d.size = this._standSize;
                this._collider2d.offset = this._standOffSet;
                this._animator.SetBool(PlayerController.AnimIsCrouching, false);
                this._unCrouch = false;
            }
        }


        private void StartDash() {
            this._isDashing = true;
            this._dashAvailable = false;

            if (this._move.x != 0) {
                this._dashDirection = new Vector2(this._move.x, 0).normalized;
            }
            else {
                this._dashDirection = this.AllSpriteFlipX ? Vector2.left : Vector2.right;
            }

            this._dashSpeed = 8f;
            this._dashTime = this._dashDuration;
        }

        private void HandleDash() {
            if (this._dashTime > 0) {
                var obstacleCount = this._body.Cast(this._dashDirection, this._contactFilter, this._hitBuffer, 0.3f);

                if (obstacleCount > 0) {
                    this._isDashing = false;
                    this._dashTime = 0;
                    this._body.linearVelocity = Vector2.zero;
                    this._velocity = Vector2.zero;
                    return;
                }

                this._body.linearVelocity = this._dashDirection * this._dashSpeed;
                this._dashTime -= Time.deltaTime;
            }
            else {
                this._isDashing = false;
                this._body.linearVelocity = Vector2.zero;
                this._velocity = Vector2.zero;
            }
        }

        private bool CanStandUp(float checkHeight) {
            var direction = Vector2.up;
            var topLeft = new Vector2((float)-0.2, 1).normalized;
            var topRight = new Vector2((float)0.2, 1).normalized;
            var hitCount = this._body.Cast(direction, this._contactFilter, this._hitBuffer, checkHeight);
            hitCount += this._body.Cast(topLeft, this._contactFilter, this._hitBuffer, checkHeight);
            hitCount += this._body.Cast(topRight, this._contactFilter, this._hitBuffer, checkHeight);
            return hitCount == 0;
        }

        private void UpdateJumpState() {
            this._jump = false;

            switch (this._jumpState) {
                case JumpState.PrepareToJump:
                    this._jumpState = JumpState.Jumping;
                    this._jump = true;
                    this._stopJump = false;
                    break;
                case JumpState.Jumping:
                    this._animator.SetBool(PlayerController.AnimJumpStart, false);
                    if (!this.IsGrounded) this._jumpState = JumpState.InFlight;
                    break;
                case JumpState.InFlight:
                    this._animator.SetBool(PlayerController.AnimIsJumping, true);
                    if (this.IsGrounded) this._jumpState = JumpState.Landed;
                    break;
                case JumpState.Landed:
                    this._jumpState = JumpState.Grounded;
                    this._animator.SetBool(PlayerController.AnimJumpEnd, true);
                    break;
            }
        }

        private void ComputeVelocity() {
            if (this._jump && this.IsGrounded) {
                this._velocity.y = this.jumpTakeOffSpeed * this.jumpModifier;
                this._jump = false;
            }
            else if (this._stopJump) {
                this._stopJump = false;
                if (this._velocity.y > 0) {
                    this._velocity.y *= this.jumpDeceleration;
                }
            }

            this._targetVelocity = this._move * this.maxSpeed;

            if (this.IsClimbing) {
                this._velocity.y = this._move.y * this.climbSpeed;
            }

            this._velocity.x = Mathf.Clamp(this._velocity.x, -this._maxSpeed, this._maxSpeed);
            this._velocity.y = Mathf.Clamp(this._velocity.y, -8f, this.jumpTakeOffSpeed);
        }


        private void FixedUpdate() {
            if (this.IsClimbing) {
                this._velocity.y = this._move.y * this.climbSpeed;
            }
            else {
                if (this._velocity.y < 0) {
                    this._velocity += Physics2D.gravity * (this.gravityModifier * Time.deltaTime);
                }
                else {
                    this._velocity += Physics2D.gravity * Time.deltaTime;
                }
            }

            this._velocity.x = this._targetVelocity.x;

            this.IsGrounded = false;

            var deltaPosition = this._velocity * Time.deltaTime;
            var moveAlongGround = new Vector2(this._groundNormal.y, -this._groundNormal.x);
            var move = moveAlongGround * deltaPosition.x;

            this.PerformMovement(move);

            move = Vector2.up * deltaPosition.y;
            this.PerformMovement(move);
        }

        private void PerformMovement(Vector2 move) {
            var distance = move.magnitude;
            if (distance > PlayerController.MinMoveDistance) {
                var count = this._body.Cast(move, this._contactFilter, this._hitBuffer,
                    distance + PlayerController.ShellRadius);
                for (var i = 0; i < count; i++) {
                    var currentNormal = this._hitBuffer[i].normal;

                    if (currentNormal.y > PlayerController.MinGroundNormalY) {
                        this.IsGrounded = true;
                        this._groundNormal = currentNormal;
                        currentNormal.x = 0;

                        if (this._velocity.y < 0) {
                            this._velocity.y = 0;
                        }
                    }

                    if (currentNormal.y < -PlayerController.MinGroundNormalY && this._velocity.y > 0) {
                        this._velocity.y = 0;
                    }

                    if (this.IsGrounded) {
                        var projection = Vector2.Dot(this._velocity, currentNormal);
                        if (projection < 0) this._velocity -= projection * currentNormal;
                    }
                    else {
                        this._velocity.x *= 0;
                    }

                    var modifiedDistance = this._hitBuffer[i].distance - PlayerController.ShellRadius;
                    distance = Mathf.Min(distance, modifiedDistance);
                }
            }

            this._body.position += move.normalized * distance;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Ladder")) {
                var contactPoint = other.ClosestPoint(this.transform.position);
                if ((contactPoint.x - (int)contactPoint.x < 0.5 && contactPoint.x - (int)contactPoint.x > 0.02) ||
                    contactPoint.x - (int)contactPoint.x > 0.98) {
                    this.AllSpriteFlipX = true;
                }
                else {
                    this.AllSpriteFlipX = false;
                }

                this.IsClimbing = true;
                this._velocity.y = 0;
            }
        }


        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag(this._ladderTag)) return;

            this.IsClimbing = false;
            this._velocity.y = 0;
        }
    }
}
