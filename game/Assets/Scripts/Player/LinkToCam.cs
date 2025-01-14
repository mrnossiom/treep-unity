using UnityEngine;

namespace Treep
{
    public class LinkToCam : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            FollowPlayer.singleton.player = this.transform;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
