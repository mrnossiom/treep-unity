using System;
using System.Collections.Generic;
using Treep.Player;
using Treep.Utils;
using Treep.Weapon;
using UnityEngine;

namespace Treep {
    public class PlayerAnimatorController : MonoBehaviour {
        [SerializeField] private PseudoDictionary<Weapons, MonoBehaviour> pseudoDictWeapons;

        [SerializeField] private GameObject headGameObject;
        [SerializeField] private GameObject bodyGameObject;

        public Dictionary<Weapons, ICloseWeapon> DictWeapons { get; private set; } = new();


        private Animator _headAnimator;
        private Dictionary<Weapons, Animator> _weaponsAnimator = new();
        private Animator _bodyAnimator;


        private SpriteRenderer _headSpriteRenderer;
        private SpriteRenderer _bodySpriteRenderer;
        private Dictionary<Weapons, SpriteRenderer> _weaponsSpritesRenderer = new();


        private Weapons CurrentWeapon { get; set; }

        private bool _flipX;

        public bool FlipX {
            get => this._flipX;
            set {
                this._headSpriteRenderer.flipX = value;
                this._bodySpriteRenderer.flipX = value;
                foreach (var sr in this._weaponsSpritesRenderer.Values) {
                    sr.flipX = value;
                }

                this._flipX = value;
            }
        }

        private Animator WeaponAnimator {
            get {
                if (this._weaponsAnimator.Count < (int)this.CurrentWeapon || // if CurrentWeapon not in weaponAnimator
                    this._weaponsAnimator[this.CurrentWeapon] is null) {
                    Debug.LogError($"Weapon {this.CurrentWeapon} animator doesn't exist");
                    return null;
                }

                return this._weaponsAnimator[this.CurrentWeapon];
            }
        }

        private SpriteRenderer WeaponSpriteRenderer {
            get {
                if (this._weaponsAnimator.Count < (int)this.CurrentWeapon || // if CurrentWeapon not in weaponAnimator
                    this._weaponsAnimator[this.CurrentWeapon] is null) {
                    Debug.LogError($"Weapon {this.CurrentWeapon} Sprite renderer doesn't exist");
                    return null;
                }

                return this._weaponsSpritesRenderer[this.CurrentWeapon];
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
            foreach (var ele in this.pseudoDictWeapons.ToActualDictionary()) {
                if (ele.Value is ICloseWeapon weapon) {
                    this.DictWeapons.Add(ele.Key, weapon);
                    Debug.Log(" Ajout de " + weapon.ToString());
                }
                else {
                    Debug.LogError(
                        "Erreur d'ajout de weapon dans Le dict de PlayerAnimatorController type : " + ele.GetType());
                }
            }

            foreach (var ele in this.DictWeapons) {
                this._weaponsAnimator.Add(ele.Key, ele.Value.Animator);
                this._weaponsSpritesRenderer.Add(ele.Key, ele.Value.SpriteRenderer);
            }

            foreach (var weapon in this._weaponsSpritesRenderer.Keys) {
                this._weaponsSpritesRenderer[weapon].enabled = false;
            }

            this.WeaponSpriteRenderer.enabled = true;

            this._headAnimator = this.headGameObject.GetComponent<Animator>();
            this._headSpriteRenderer = this.headGameObject.GetComponent<SpriteRenderer>();

            this._bodyAnimator = this.bodyGameObject.GetComponent<Animator>();
            this._bodySpriteRenderer = this.bodyGameObject.GetComponent<SpriteRenderer>();
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
                this._headAnimator.SetBool("IsMoving", value);
                this.WeaponAnimator.SetBool("IsMoving", value);
                this._bodyAnimator.SetBool("IsMoving", value);
            }

            this._headAnimator.SetBool(id, value);
            this.WeaponAnimator.SetBool(id, value);
            this._bodyAnimator.SetBool(id, value);
        }

        public void SetTrigger(int id) {
            Debug.Log($"Set trigger {id}");
        }

        public void SetFloat(int id, float value) {
            Debug.Log($"Set float {id} {value}");
        }
    }
}
