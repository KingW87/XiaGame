using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 简单对象池 - 减少Instantiate/Destroy开销
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance;
        
        private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        
        private void Awake()
        {
            Instance = this;
        }
        
        /// <summary>
        /// 注册预制体到池中
        /// </summary>
        public void RegisterPrefab(string key, GameObject prefab)
        {
            if (!prefabs.ContainsKey(key))
            {
                prefabs[key] = prefab;
                pool[key] = new Queue<GameObject>();
            }
        }
        
        /// <summary>
        /// 生成对象
        /// </summary>
        public T Spawn<T>(string key, Vector2 position) where T : MonoBehaviour
        {
            GameObject obj = null;
            
            if (pool.ContainsKey(key) && pool[key].Count > 0)
            {
                obj = pool[key].Dequeue();
                obj.transform.position = position;
                obj.SetActive(true);
            }
            else if (prefabs.ContainsKey(key))
            {
                obj = Instantiate(prefabs[key], position, Quaternion.identity);
            }
            
            if (obj != null)
            {
                return obj.GetComponent<T>();
            }
            
            return null;
        }
        
        /// <summary>
        /// 回收对象
        /// </summary>
        public void Despawn(string key, GameObject obj)
        {
            obj.SetActive(false);
            
            if (!pool.ContainsKey(key))
            {
                pool[key] = new Queue<GameObject>();
            }
            
            pool[key].Enqueue(obj);
        }
    }
}
