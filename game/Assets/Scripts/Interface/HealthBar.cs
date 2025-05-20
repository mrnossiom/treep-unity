using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface
{
    public class HealthBar : MonoBehaviour
    {
        public Image fillImage;
        public Player.Player localPlayer;

        private float displayedHealth;
        public float animationSpeed = 40f;

        void Start()
        {
            localPlayer = FindObjectsByType<Player.Player>(FindObjectsSortMode.None).First(player => player.isLocalPlayer);
            displayedHealth = localPlayer.health;
            UpdateFillInstant();
        }

        void Update()
        {
            float targetHealth = localPlayer.health;
            
            if (Mathf.Abs(displayedHealth - targetHealth) > 0.01f)
            {
                displayedHealth = Mathf.MoveTowards(displayedHealth, targetHealth, animationSpeed * Time.deltaTime);
                UpdateFill();
            }
        }

        void UpdateFill()
        {
            fillImage.fillAmount = displayedHealth / localPlayer.maxHealth;
        }

        void UpdateFillInstant()
        {
            fillImage.fillAmount = localPlayer.health / localPlayer.maxHealth;
        }
    }
}