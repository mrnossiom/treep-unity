using System;
using Mirror;
using UnityEngine;

namespace Treep.IA
{
    public class RollingStone : MonoBehaviour
    {
        public float speed = 8f;
        public float damage = 5f;

        void Start()
        {
            Destroy(gameObject, 3f);
        }

        void Update()
        {
            transform.position += Vector3.left * (this.speed * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.gameObject.GetComponent<Player.Player>();
                if (player != null)
                {
                    player.CmdTakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}
