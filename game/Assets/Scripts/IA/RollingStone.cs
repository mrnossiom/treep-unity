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
            transform.position += Vector3.left * speed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D collision)
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
        }
    }
}
