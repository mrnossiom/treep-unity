using System.Collections;

using UnityEngine;

namespace Treep
{
    public class BackgroundController : MonoBehaviour
    {

        private float startPos, length;
        public GameObject cam;
        public float backgroundSpeed; //The intensity of the parallax effect 0 -> 1 (0 move a lot and 1 don't move)

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            startPos = transform.position.x;
            length = 64; //GetComponent<SpriteRenderer>().size.x is the line to not write magic number, but this line don't work idk why
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float distance = cam.transform.position.x * backgroundSpeed;
            float movement = cam.transform.position.x * (1 - backgroundSpeed);

            transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

            if(movement > startPos + length)
            {
                startPos += length;
            }
            else if(movement < startPos - length)
            {
                startPos -= length;
            }
        }
    }
}
