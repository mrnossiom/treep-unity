using Mirror;
using Treep.SFX;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Treep.Player;
using Vector2 = UnityEngine.Vector2;

namespace Treep.Player {
    public enum JumpState {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

    public enum LookDirection {
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
        [SerializeField] private float jumpDeceleration = 0.4f;

        [SerializeField] private float climbSpeed = 3f;
        [SerializeField] private bool onTopOfLadder;

        public static Weapons StartWeapon = Weapons.Fist;

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

        private Vector2 _standSize = new(1.4f, 3.2f);
        private Vector2 _crouchSize = new(1.4f, 2.3f);
        private Vector2 _standOffSet = new(-0.18f, -0.126588f);
        private Vector2 _crouchOffSet = new(-0.18f, -0.5325f);
        private bool _unCrouch;

        public bool IsDashing { get; set; }
        private bool _dashAvailable;
        private float _dashSpeed;
        private float _dashDuration = 0.4f;
        private Vector2 _dashDirection;
        private float _dashTime;
        private Vector2 _velocity;
        private float _dashCooldown = 0.8f;
        private float _lastDashTime;
        public bool isCrouching;

        public Transform _dashEffectPoint; //engros ca c'est en fonction de l'endroit du player
        public Vector2 dashEffectPos; // et ca c'est overall
        private Vector2 _currentDashEffectPos; // et ca c'est current pos

        private Animator _dashAnimator;
        private SpriteRenderer _dashSpriteRenderer;
        private SpriteRenderer _closeAttackRenderer;
        public PlayerCombat scriptPlayerCombat;

        private PlayerAnimatorController _animatorController;

        [SerializeField] private AudioClip ladderSoundClip;
        [SerializeField] private AudioMixer audioMixer;
        private AudioSource climbingAudioSource;
        [SerializeField] private AudioClip walkingSoundClip;
        private AudioSource walkingAudioSource;
        [SerializeField] private AudioClip dashSoundClip;

        private bool IsClimbing { get; set; }

        public LookDirection lookDirection;

        [SyncVar(hook = nameof(OnFlipChanged))]
        private bool _allSpriteFlipX;
        
        private void OnFlipChanged(bool oldValue, bool newValue) {
            //_spriteRenderer.flipX = newValue;
            _dashSpriteRenderer.flipX = newValue;
            _closeAttackRenderer.flipX = newValue;
            this._animatorController.FlipX = newValue;
        }

        [Command]
        private void CmdSetFlipX(bool value) {
            _allSpriteFlipX = value;
        }

        private bool AllSpriteFlipX {
            get => this._allSpriteFlipX;
            set {
                if (isLocalPlayer) {
                    CmdSetFlipX(value);
                }
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
            this._animatorController = this.GetComponent<PlayerAnimatorController>();
            this._animatorController.SetPlayerStates(this);

            this._ladderTag = TagHandle.GetExistingTag("Ladder");

            this._contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(this.gameObject.layer));
            this._dashAnimator = this._dashEffectPoint.GetComponent<Animator>();
            this._dashSpriteRenderer = this._dashEffectPoint.GetComponent<SpriteRenderer>();
            this._closeAttackRenderer = this.scriptPlayerCombat.attackPoint.GetComponent<SpriteRenderer>();
        }

        private void OnSpriteFlip(bool _, bool newValue) {
            this.AllSpriteFlipX = newValue;
        }

        private void Update() {
            if (!this.isLocalPlayer) return;

            if (this._controlEnabled) {
                this.UpdateLooking();
                this.UpdateCrouch();
                this.UpdateJump();
                this.UpdateClimb();
                this.UpdateWalkSound();
                this.UpdateDash();
                this.UpdateHitboxCollider();
            }
            else {
                this._move.x = 0;
                this._move.y = 0;
            }

            this._targetVelocity = Vector2.zero;
            this.ComputeVelocity();
        }

        private void UpdateHitboxCollider() { }

        private void UpdateDash() {
            if (this.IsGrounded && Time.time >= this._lastDashTime + this._dashCooldown) {
                this._dashAvailable = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && !this.IsDashing && this._dashAvailable) {
                this._animatorController.TriggerDash();
                this._dashAnimator.SetTrigger("Dash");
                this._currentDashEffectPos = this.AllSpriteFlipX
                    ? new Vector3(this.transform.position.x + this.dashEffectPos.x,
                        this.transform.position.y + this.dashEffectPos.y)
                    : new Vector3(this.transform.position.x - this.dashEffectPos.x,
                        this.transform.position.y + this.dashEffectPos.y);
                this.StartDash();
                this._lastDashTime = Time.time;
            }

            if (this.IsDashing) {
                this.HandleDash();
            }

            this._dashEffectPoint.position
                = new Vector3(this._currentDashEffectPos.x,
                    this._currentDashEffectPos.y);
        }


        private void UpdateWalkSound() {
            var isWalking = this.IsGrounded && Mathf.Abs(this._move.x) > 0.01f && !this.isCrouching &&
                            !this.IsDashing && !this.IsClimbing;

            if (isWalking) {
                if (this.walkingAudioSource == null) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    this.walkingAudioSource
                        = SoundFXManager.Instance.PlayLoopingSound(this.walkingSoundClip, this.transform, soundLevel);
                }
            }
            else {
                if (this.walkingAudioSource != null) {
                    SoundFXManager.Instance.StopLoopingSound(this.walkingAudioSource);
                    this.walkingAudioSource = null;
                }
            }
        }

        private void UpdateClimb() {
            this._animatorController.UpdateClimb(this.IsClimbing, this._move.y);
            if (this.IsClimbing && Mathf.Abs(this._move.y) > 0.01f) {
                if (this.climbingAudioSource == null) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    this.climbingAudioSource
                        = SoundFXManager.Instance.PlayLoopingSound(this.ladderSoundClip, this.transform, soundLevel);
                }
            }
            else {
                if (this.climbingAudioSource != null) {
                    SoundFXManager.Instance.StopLoopingSound(this.climbingAudioSource);
                    this.climbingAudioSource = null;
                }
            }
        }

        private void UpdateJump() {
            if (this._jumpState == JumpState.Grounded && Input.GetButtonDown("Jump")) {
                //this._animatorController.SetBool(PlayerController.AnimJumpStart, true);
                //this._animatorController.SetBool(PlayerController.AnimIsJumping, false);
                //this._animatorController.SetBool(PlayerController.AnimJumpEnd, false);
                this._animatorController.TriggerJump();
                this._jumpState = JumpState.PrepareToJump;
            }
            else if (Input.GetButtonUp("Jump")) this._stopJump = true;

            this.UpdateJumpState();
        }

        private void UpdateLooking() {
            this._move.x = Input.GetAxis("Horizontal");
            this._move.y = Input.GetAxis("Vertical");
            this._animatorController.UdpateMouv(this._move.x != 0);
            if (this.IsClimbing && !this.onTopOfLadder) {
                this._move.x = 0;
                if (Input.GetKeyDown(KeyCode.Space)) {
                    this.IsClimbing = false;
                }
            }

            if (this._move.y != 0) // si on cible le haut ou le bas 
            {
                if (this._move.y > 0) // cible top
                {
                    this.lookDirection = LookDirection.Top;
                }
                else if (this._move.y < 0) // cible bottom
                {
                    this.lookDirection = LookDirection.Bottom;
                }

                if (this._move.x != 0 && !this.IsClimbing) {
                    this.AllSpriteFlipX = this._move.x < 0;
                }
            }
            else // le player vas que a gauche ou a droite (ou rien)
            {
                if (this._move.x > 0 && !this.IsClimbing) {
                    this.AllSpriteFlipX = false;
                }
                else if (this._move.x < 0 && !this.IsClimbing) {
                    this.AllSpriteFlipX = true;
                }

                this.lookDirection = this.AllSpriteFlipX ? LookDirection.Left : LookDirection.Right;
            }
        }

        private void UpdateCrouch() {
            if (Input.GetKeyDown(KeyCode.C)) {
                this.isCrouching = true;
                this._collider2d.size = this._crouchSize;
                this._collider2d.offset = this._crouchOffSet;
                this._animatorController.UpdateCrouch(true);
            }

            if (Input.GetKeyUp(KeyCode.C)) {
                this._unCrouch = true;
            }

            if (this._unCrouch && this.CanStandUp(1.2f)) {
                this.isCrouching = false;
                this._collider2d.size = this._standSize;
                this._collider2d.offset = this._standOffSet;
                this._animatorController.UpdateCrouch(false);
                this._unCrouch = false;
            }

            this._animatorController.UpdateSpeedCrouch(this._move.x);
        }


        private void StartDash() {
            this.IsDashing = true;
            this._dashAvailable = false;
            this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
            soundLevel = (soundLevel + 80) / 100;
            SoundFXManager.Instance.PlaySoundFXClip(this.dashSoundClip, this.transform, soundLevel);
            if (this._move.x != 0) {
                if (this._move.y != 0) {
                    this._dashDirection = new Vector2(this._move.x, this._move.y).normalized;
                }
                else {
                    this._dashDirection = new Vector2(this._move.x, 0).normalized;
                }
            }
            else {
                this._dashDirection = this.AllSpriteFlipX ? Vector2.left : Vector2.right;
            }

            this._dashSpeed = 8f * Player.Singleton.dashMultiplier;
            this._dashTime = this._dashDuration * Player.Singleton.dashMultiplier;
        }

        private void HandleDash() {
            if (this._dashTime > 0) {
                var obstacleCount = this._body.Cast(this._dashDirection, this._contactFilter, this._hitBuffer, 0.3f);

                if (obstacleCount > 0) {
                    this.IsDashing = false;
                    this._dashTime = 0;
                    this._body.linearVelocity = Vector2.zero;
                    this._velocity = Vector2.zero;
                    return;
                }

                this._body.linearVelocity = this._dashDirection * this._dashSpeed;
                this._dashTime -= Time.deltaTime;
            }
            else {
                this.IsDashing = false;
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
                    //this._animatorController.SetBool(PlayerController.AnimJumpStart, false);
                    if (!this.IsGrounded) this._jumpState = JumpState.InFlight;
                    break;
                case JumpState.InFlight:
                    //this._animatorController.SetBool(PlayerController.AnimIsJumping, true);
                    if (this.IsGrounded) this._jumpState = JumpState.Landed;
                    break;
                case JumpState.Landed:
                    //this._animatorController.SetBool(PlayerController.AnimJumpEnd, true);
                    this._jumpState = JumpState.Grounded;
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

            if (this.isCrouching) {
                this._targetVelocity = this._move * this.maxSpeed / 2;
            }

            else {
                this._targetVelocity = this._move * this.maxSpeed;
            }

            if (this.IsClimbing) {
                this._velocity.y = this._move.y * this.climbSpeed;
            }
        }


        private void FixedUpdate() {
            if (this._velocity.y < 0) {
                this._velocity += Physics2D.gravity * (this.gravityModifier * Time.deltaTime);
            }
            else {
                this._velocity += Physics2D.gravity * Time.deltaTime;
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
            if (other.CompareTag("Ladder") && !this.IsGrounded && !this.IsDashing) {
                var contactPoint = other.ClosestPoint(this.transform.position);
                if (contactPoint.y < this.transform.position.y) {
                    this.onTopOfLadder = true;
                }

                if (contactPoint.x - (int)contactPoint.x <= 0.03) {
                    contactPoint = new Vector2(contactPoint.x - 0.05f, contactPoint.y);
                }

                if (contactPoint.x - (int)contactPoint.x >= 0.97) {
                    contactPoint = new Vector2(contactPoint.x + 0.05f, contactPoint.y);
                }

                if (contactPoint.x - (int)contactPoint.x < 0.5) {
                    this.OnSpriteFlip(false, true);
                    this._velocity.x = 0;
                    this.transform.position = new Vector3((int)contactPoint.x + 0.9f, this.transform.position.y, 0);
                }
                else {
                    this.OnSpriteFlip(true, false);
                    this._velocity.x = 0;
                    this.transform.position = new Vector3((int)contactPoint.x + 0.4f, this.transform.position.y, 0);
                }

                this.IsClimbing = true;
                this._velocity.y = 0;
            }

            if (other.CompareTag("Spikes")) {
                Player.Singleton.CmdTakeDamage(5000f);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag(this._ladderTag)) return;
            this.IsClimbing = false;
            this.onTopOfLadder = false;
            this._velocity.y = 0;
        }
    }
}
