using System;
using TMPro;
using Treep.SFX;
using Treep.Weapon;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace Treep {
    public class ShopMenu : MonoBehaviour {
        public bool isActive = false;

        private Player.Player _currentPlayer;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button stickButton;
        [SerializeField] private Button spearButton;
        [SerializeField] private Button swordButton;
        [SerializeField] private Button healthButton;
        [SerializeField] private Button visionButton;
        [SerializeField] private Button damageButton;
        [SerializeField] private Button dashButton;

        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI visionText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI dashText;
        [SerializeField] private TextMeshProUGUI weapon;

        [SerializeField] private TextMeshProUGUI healthPriceText;
        [SerializeField] private TextMeshProUGUI visionPriceText;
        [SerializeField] private TextMeshProUGUI damagePriceText;
        [SerializeField] private TextMeshProUGUI dashPriceText;

        private int _stickPrice;
        private int _spearPrice;
        private int _swordPrice;
        private int _healthPrice;
        private int _visionPrice;
        private int _damagePrice;
        private int _dashPrice;
        private int _inflation;
        
        [SerializeField] private AudioClip Kaching;
        [SerializeField] private AudioClip Error;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioClip buttonPress;

        public void SetPlayer(Player.Player player) {
            this._currentPlayer = player;
        }

        private void Start() {
            this._stickPrice = 10;
            this._spearPrice = 20;
            this._swordPrice = 25;
            this._healthPrice = 15;
            this._visionPrice = 10;
            this._damagePrice = 15;
            this._dashPrice = 10;
            this._inflation = 5;

            this.closeButton.onClick.AddListener(() => {
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                soundLevel = (soundLevel + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.buttonPress, this.transform, soundLevel);
                this.gameObject.SetActive(false);
            });
            this.stickButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._stickPrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.CmdChangeWeapon(Weapons.Stick);
                this.weapon.text = "Weapon: Stick";

                this._currentPlayer.money -= this._stickPrice;
                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._stickPrice += this._inflation;
            });
            this.spearButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._spearPrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.CmdChangeWeapon(Weapons.Spear);
                this.weapon.text = "Weapon: Spear";

                this._currentPlayer.money -= this._spearPrice;
                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._spearPrice += this._inflation;
            });
            this.swordButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._swordPrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.CmdChangeWeapon(Weapons.Sword);
                this.weapon.text = "Weapon: Sword";

                this._currentPlayer.money -= this._swordPrice;
                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._swordPrice += this._inflation;
            });
            this.healthButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._healthPrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                    
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.maxHealth += 5;
                this._currentPlayer.health = this._currentPlayer.maxHealth;
                this.healthText.text = $"Max Health: {this._currentPlayer.health.ToString()}";
                this.healthPriceText.text = $"Health Boost - {this._healthPrice} Bones";

                this._currentPlayer.money -= this._healthPrice;
                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._healthPrice += this._inflation;
            });
            this.visionButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._visionPrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.visionMultiplier += (float)0.05;
                this._currentPlayer.IncreaseVision();
                this.visionText.text = $"Vision Multiplier: {this._currentPlayer.visionMultiplier.ToString()}";
                this.visionPriceText.text = $"Vision Boost - {this._visionPrice} Bones";

                this._currentPlayer.money -= this._visionPrice;
                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._visionPrice += this._inflation;
            });
            this.damageButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._damagePrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.damageMultiplier += (float)0.05;
                this.damageText.text = $"Damage Multiplier: {this._currentPlayer.damageMultiplier.ToString()}";
                this.damagePriceText.text = $"Damage Boost - {this._damagePrice} Bones";

                this._currentPlayer.money -= this._damagePrice;
                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._damagePrice += this._inflation;
            });
            this.dashButton.onClick.AddListener(() => {
                if (this._currentPlayer.money < this._dashPrice) {
                    this.audioMixer.GetFloat("SFXVolume", out var soundLevel);
                    soundLevel = (soundLevel + 80) / 100;
                    SoundFXManager.Instance.PlaySoundFXClip(this.Error, this.transform, soundLevel);
                    return;
                };
                this.audioMixer.GetFloat("SFXVolume", out var soundLevel2);
                soundLevel2 = (soundLevel2 + 80) / 100;
                SoundFXManager.Instance.PlaySoundFXClip(this.Kaching, this.transform, soundLevel2);
                this._currentPlayer.dashMultiplier += (float)0.05;
                this.dashText.text = $"Dash Multiplier: {this._currentPlayer.dashMultiplier.ToString()}";
                this.dashPriceText.text = $"Dash Boost - {this._dashPrice} Bones";

                this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
                this._currentPlayer.money -= this._dashPrice;
                this._dashPrice += this._inflation;
            });
        }

        public void Awake() {
            this.moneyText.text = $"Bones (money): {this._currentPlayer.money.ToString()}";
        }

        public void SetActive(bool active) {
            this.isActive = active;
            this.gameObject.SetActive(active);
        }
    }
}
