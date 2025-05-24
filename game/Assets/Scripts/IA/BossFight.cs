using System;
using UnityEngine;

namespace Treep
{
    using UnityEngine;
    using Mirror;

    public class BossRockSpawner : MonoBehaviour
    {
        public GameObject objectToSpawn;
        public float minXDistance = 3f;
        public float maxXDistance = 10f;
        public float spawnHeight = 10f;

        private float timer;

        private float nextSpawnTime;

        void Update()
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnObject();
                nextSpawnTime = Time.time + Random.Range(0.2f, 2f);
            }
        }

        void SpawnObject()
        {
            if (objectToSpawn == null) return;

            float direction = Random.value < 0.5f ? -1f : 1f;
            float distance = Random.Range(minXDistance, maxXDistance);
            float spawnX = transform.position.x + direction * distance;
            float spawnY = transform.position.y + spawnHeight;

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        }
    }

}
