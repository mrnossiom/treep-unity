using System.Collections.Generic;
using UnityEngine;

namespace Treep.Utils
{
    public class InitializeOnce : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objects;

        static bool _initialized = false;

        void Awake()
        {
            if (_initialized) return;
            _initialized = true;

            foreach (var obj in objects)
            {
                var target = Instantiate(obj);
                DontDestroyOnLoad(target);
            }

            Destroy(gameObject);
        }
    }
}
