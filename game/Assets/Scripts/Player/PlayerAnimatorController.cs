using System;
using System.Collections.Generic;
using Treep.Player;
using Treep.Weapon;
using UnityEngine;

namespace Treep {
    public class PlayerAnimatorController : MonoBehaviour {
        public Animator headAnimator;
        public List<Animator> weaponsAnimator;
        public Animator bodyAnimator;


        private SpriteRenderer _headSpriteRenderer;
        private SpriteRenderer _bodySpriteRenderer;
        private List<SpriteRenderer> _weaponsSpritesRenderer = new();


        private Weapons CurrentWeapon { get; set; }

        private bool _flipX;

        public bool FlipX {
            get => this._flipX;
            set {
                this._headSpriteRenderer.flipX = value;
                this._bodySpriteRenderer.flipX = value;
                foreach (var sr in this._weaponsSpritesRenderer) {
                    sr.flipX = value;
                }

                this._flipX = value;
            }
        }

        private Animator WeaponAnimator {
            get {
                if (this.weaponsAnimator.Count < (int)this.CurrentWeapon || // if CurrentWeapon not in weaponAnimator
                    this.weaponsAnimator[(int)this.CurrentWeapon] is null) {
                    Debug.LogError($"Weapon {this.CurrentWeapon} animator doesn't exist");
                    return null;
                }

                return this.weaponsAnimator[(int)this.CurrentWeapon];
            }
        }

        private SpriteRenderer WeaponSpriteRenderer {
            get {
                if (this.weaponsAnimator.Count < (int)this.CurrentWeapon || // if CurrentWeapon not in weaponAnimator
                    this.weaponsAnimator[(int)this.CurrentWeapon] is null) {
                    Debug.LogError($"Weapon {this.CurrentWeapon} Sprite renderer doesn't exist");
                    return null;
                }

                return this._weaponsSpritesRenderer[(int)this.CurrentWeapon];
            }
        }


        private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int AnimJumpStart = Animator.StringToHash("JumpStart");
        private static readonly int AnimIsJumping = Animator.StringToHash("IsJumping");
        private static readonly int AnimJumpEnd = Animator.StringToHash("JumpEnd");
        private static readonly int AnimIsCrouching = Animator.StringToHash("IsCrouching");
        private static readonly int AnimIsClimbing = Animator.StringToHash("IsClimbing");
        private static readonly int AnimClimbSpeed = Animator.StringToHash("ClimbSpeed");
        private static readonly int AnimIsDashing = Animator.StringToHash("IsDashing");
        //private static readonly int AnimWeapon = Animator.StringToHash("CurrentWeapon");


        private PlayerController _playerController;

        public bool IsCrouching => this._playerController.isCrouching;


        public void Awake() {
            this.CurrentWeapon = PlayerController.StartWeapon;
            foreach (var animator in this.weaponsAnimator) {
                var sp = animator.gameObject.GetComponent<SpriteRenderer>();
                sp.enabled = false;
                this._weaponsSpritesRenderer.Add(sp);
            }

            this.WeaponSpriteRenderer.enabled = true;

            this._headSpriteRenderer = this.headAnimator.gameObject.GetComponent<SpriteRenderer>();
            this._bodySpriteRenderer = this.bodyAnimator.gameObject.GetComponent<SpriteRenderer>();
        }

        public void SetPlayerStates(PlayerController playerController) {
            this._playerController = playerController;
        }


        public void SwitchWeapon(Weapons newWeapons) {
            this.WeaponAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            this.CurrentWeapon = newWeapons;
            this.WeaponAnimator.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }


        public void SetBool(int id, bool value) {
            //legacy
            if (id == PlayerAnimatorController.AnimIsMoving) {
                this.headAnimator.SetBool("IsMoving", value);
                this.WeaponAnimator.SetBool("IsMoving", value);
                this.bodyAnimator.SetBool("IsMoving", value);
            }

            this.headAnimator.SetBool(id, value);
            this.WeaponAnimator.SetBool(id, value);
            this.bodyAnimator.SetBool(id, value);
        }
    }
}
