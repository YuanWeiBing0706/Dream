using System;
using System.Collections.Generic;
using System.Linq;
using DreamManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DreamPool
{
    /// <summary>
    /// 对象池管理器（支持 VContainer 注入）。
    /// <para>保留原有设计：场景池 + 持久池</para>
    /// </summary>
    public class PoolManager
    {
        private readonly ResourcesManager _resourcesManager;

        private List<Pool> _pools = new List<Pool>();
        private List<Pool> _persistentPools = new List<Pool>();

        private GameObject _scenePoolParent;
        private GameObject _persistentPoolParent;

        public PoolManager(ResourcesManager resourcesManager)
        {
            _resourcesManager = resourcesManager;
        }

        public void ClearScenePool()
        {
            _scenePoolParent = null;
            _pools.Clear();
        }

        public GameObject Release(string objectID, Vector3 position, Quaternion identity)
        {
            if (_scenePoolParent == null)
            {
                _scenePoolParent = new GameObject("Pools");
            }

            Pool pool = _pools.Find(p => p.poolName == objectID);
            if (pool == null)
            {
                var go = _resourcesManager.LoadAsset<GameObject>(objectID);
                if (go == null)
                {
                    Debug.LogWarning($"Object is not Exist {objectID}");
                }

                int poolSize = 1;

                pool = new Pool(poolName: objectID, poolSize, pref: go, _scenePoolParent.transform);
                _pools.Add(pool);
            }

            GameObject spawnedObj = pool.poolObjects.Peek();
            if (spawnedObj == null || spawnedObj.activeSelf)
            {
                spawnedObj = Object.Instantiate(pool.pref, pool.objectsParent.transform, true);
            }
            else
            {
                pool.poolObjects.Dequeue();
            }

            spawnedObj.transform.position = position;
            spawnedObj.transform.rotation = identity;
            spawnedObj.SetActive(true);
            pool.poolObjects.Enqueue(spawnedObj);
            return spawnedObj;
        }

        public GameObject Release(string objectID, Transform parent)
        {
            if (_scenePoolParent == null)
            {
                _scenePoolParent = new GameObject("Pools");
            }

            Pool pool = _pools.Find(p => p.poolName == objectID);
            if (pool == null)
            {
                var go = _resourcesManager.LoadAsset<GameObject>(objectID);
                if (go == null)
                {
                    Debug.LogWarning($"Object is not Exist {objectID}");
                }

                int poolSize = 1;

                pool = new Pool(poolName: objectID, poolSize, pref: go, _scenePoolParent.transform);
                _pools.Add(pool);
            }

            GameObject spawnedObj = pool.poolObjects.Peek();
            if (spawnedObj == null || spawnedObj.activeSelf)
            {
                spawnedObj = Object.Instantiate(pool.pref, pool.objectsParent.transform, true);
            }
            else
            {
                pool.poolObjects.Dequeue();
            }
            spawnedObj.transform.SetParent(parent);
            spawnedObj.transform.localPosition = Vector3.zero;
            spawnedObj.transform.rotation = Quaternion.identity;
            spawnedObj.transform.localScale = Vector3.one;
            spawnedObj.SetActive(true);
            pool.poolObjects.Enqueue(spawnedObj);
            return spawnedObj;
        }

        public void Recycle(string objectID, GameObject go)
        {
            Pool pool = _pools.Find(p => p.poolName == objectID);
            if (pool.objectsParent == null)
            {
                Debug.LogError($"{go.name} has no gameobject");
            }
            go.transform.SetParent(pool.objectsParent.transform);
            go.SetActive(false);
        }

        public GameObject ReleasePersistent(string objectID, Vector3 position, Quaternion identity)
        {
            if (_persistentPoolParent == null)
            {
                _persistentPoolParent = new GameObject("PersistentPools");
                Object.DontDestroyOnLoad(_persistentPoolParent);
            }

            Pool pool = _persistentPools.Find(p => p.poolName == objectID);
            if (pool == null)
            {
                var go = _resourcesManager.LoadAsset<GameObject>(objectID);
                if (go == null)
                {
                    Debug.LogWarning($"Object is not Exist {objectID}");
                }

                int poolSize = 1;

                pool = new Pool(poolName: objectID, poolSize, pref: go, _persistentPoolParent.transform);
                _persistentPools.Add(pool);
            }

            GameObject spawnedObj = pool.poolObjects.Peek();
            if (spawnedObj == null || spawnedObj.activeSelf)
            {
                spawnedObj = Object.Instantiate(pool.pref, pool.objectsParent.transform, true);
            }
            else
            {
                pool.poolObjects.Dequeue();
            }

            spawnedObj.transform.position = position;
            spawnedObj.transform.rotation = identity;
            spawnedObj.SetActive(true);
            pool.poolObjects.Enqueue(spawnedObj);
            return spawnedObj;
        }

        public void RecyclePersistent(string objectID, GameObject go)
        {
            Pool pool = _persistentPools.Find(p => p.poolName == objectID);
            go.transform.SetParent(pool.objectsParent.transform);
            go.SetActive(false);
        }
    }
}