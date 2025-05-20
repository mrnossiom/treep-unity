using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Treep.Interface
{
    public class HealthBar : MonoBehaviour
    {
        public Image fillImage;
        public Player.Player localPlayer;
        
        void Start() {
            localPlayer = FindObjectsByType<Player.Player>(FindObjectsSortMode.None).First(player => player.isLocalPlayer);
        }
        
        void Update()
        {
            this.fillImage.fillAmount = this.localPlayer.health / this.localPlayer.maxHealth;
        }
    }
}
