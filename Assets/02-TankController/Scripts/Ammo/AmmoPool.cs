using System.Collections.Generic;
using UnityEngine;

namespace _02_TankController.Scripts.Ammo
{
    public class AmmoPool : MonoBehaviour
    {
        [System.Serializable]
        public struct PoolDefinition
        {
            public BulletType Type;
            public BaseBullet Prefab;
            public int DefaultSize;
        }

        [SerializeField] private List<PoolDefinition> m_PoolSetup;

        // Dictionary to map the Enum to the actual Queue of bullets
        private Dictionary<BulletType, Queue<BaseBullet>> m_Pools = new Dictionary<BulletType, Queue<BaseBullet>>();
        
        // Dictionary to remember the default size limit for the shrinking logic
        private Dictionary<BulletType, int> m_PoolLimits = new Dictionary<BulletType, int>();

        private void Awake()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var def in m_PoolSetup)
            {
                Queue<BaseBullet> newQueue = new Queue<BaseBullet>();
                
                // Create the startup ammo
                for (int i = 0; i < def.DefaultSize; i++)
                {
                    BaseBullet b = CreateBullet(def.Prefab);
                    newQueue.Enqueue(b);
                }

                m_Pools.Add(def.Type, newQueue);
                m_PoolLimits.Add(def.Type, def.DefaultSize);
            }
        }

        private BaseBullet CreateBullet(BaseBullet prefab)
        {
            BaseBullet b = Instantiate(prefab, transform);
            b.gameObject.SetActive(false);
            return b;
        }

        public BaseBullet GetBullet(BulletType type)
        {
            if (!m_Pools.ContainsKey(type)) return null;

            Queue<BaseBullet> queue = m_Pools[type];

            // 1. Check if we have ammo ready
            if (queue.Count > 0)
            {
                BaseBullet b = queue.Dequeue();
                b.gameObject.SetActive(true);
                return b;
            }

            // 2. Pool is empty! Dynamic Expansion Time.
            // Find the prefab for this type
            var def = m_PoolSetup.Find(x => x.Type == type);
            BaseBullet newBullet = CreateBullet(def.Prefab);
            newBullet.gameObject.SetActive(true);
            
            // Note: We don't enqueue it here. We give it to the player.
            // It will be handled when it tries to return.
            return newBullet;
        }

        public void ReturnBullet(BaseBullet bullet, BulletType type)
        {
            // SAGE LOGIC: The "Rubber Band" Shrink
            Queue<BaseBullet> queue = m_Pools[type];
            int limit = m_PoolLimits[type];

            // If we have more than the default capacity, DESTROY the extra.
            // This slowly shrinks the pool back to default size automatically.
            if (queue.Count >= limit)
            {
                Destroy(bullet.gameObject);
            }
            else
            {
                bullet.gameObject.SetActive(false);
                queue.Enqueue(bullet);
            }
        }
    }
}