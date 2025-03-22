using UnityEngine;

namespace Treep
{
    public class BackgroundController : MonoBehaviour
    {

        private float startPos;
        public GameObject cam;
        public float backgroundSpeed; //The intensity of the parallax effect 0 -> 1 (0 move a lot and 1 don't move)

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            startPos = transform.position.x;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float distance = cam.transform.position.x * backgroundSpeed;
            transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
        }
    }
}
