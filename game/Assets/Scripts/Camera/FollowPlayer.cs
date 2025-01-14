using UnityEngine;

namespace Treep
{
    public class FollowPlayer : MonoBehaviour
    {
        public Transform player;
        public static FollowPlayer singleton { get; private set; }

        void Awake()
        {
            if(singleton == null)
            {
                singleton = this;
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (player == null) return;

            var pos = transform.position;
            pos.x = player.transform.position.x;
            pos.y = player.transform.position.y;
            transform.position = pos;
        }
    }
}
