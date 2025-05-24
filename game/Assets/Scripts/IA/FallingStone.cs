using System;
using Mirror;
using UnityEngine;

namespace Treep.IA
{
    public class FallingStone : MonoBehaviour {

        public float damage = 5f;

        private void Start() {
            Destroy(gameObject, 2f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

            if (collision.gameObject.CompareTag("Player"))
            {
                var player = collision.gameObject.GetComponent<Player.Player>();
                if (player != null)
                {
                    player.CmdTakeDamage(damage);
                }
                Destroy(gameObject);
            }

            if ( collision.gameObject.CompareTag("Boss")) {
                Destroy(gameObject);
            }
            
        }
    }
}
