using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DreamPool
{
    public class Pool
    {
        public string poolName;
        public int poolSize;
        public GameObject pref;
        public GameObject objectsParent;
        [HideInInspector] public Queue<GameObject> poolObjects = new Queue<GameObject>();


        public Pool(string poolName, int poolSize, GameObject pref, Transform poolParent)
        {
            objectsParent = new GameObject(poolName);
            objectsParent.transform.SetParent(poolParent);
            this.poolName = poolName;
            this.poolSize = poolSize;
            this.pref = pref;
            for (int i = 0; i < poolSize; i++)
            {
                var go = GameObject.Instantiate(pref, Vector3.zero, quaternion.identity);
                go.transform.SetParent(objectsParent.transform);
                go.SetActive(false);
                poolObjects.Enqueue(go);
            }

        }

        public void Init(Transform poolParent)
        {
            var parent = new GameObject(poolName);
            parent.transform.SetParent(poolParent);
            for (int i = 0; i < poolSize; i++)
            {
                var go = GameObject.Instantiate(pref, Vector3.zero, quaternion.identity);
                go.transform.SetParent(parent.transform);
                go.SetActive(false);
                poolObjects.Enqueue(go);
            }
        }
    }
}
