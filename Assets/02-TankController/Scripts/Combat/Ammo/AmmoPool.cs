#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts.Combat.Ammo
{
    public class AmmoPool : MonoBehaviour
    {
        [Serializable]
        public struct PoolDefinition
        {
            public BulletType Type;
            public BaseBullet Prefab;
            public int DefaultSize;
        }

        //Pool definition just stores the pool label, the actual object to be spawned in the pool and the size of the pool
        [SerializeField] private List<PoolDefinition> m_PoolSetup;

        // Dictionary used to essentially give the object pool a type label
        // This uses the bullet type as the key and the queue of the bullets as the value
        // This means any derived class of BaseBullet can be stored here e.g. FMJ
        public Dictionary<BulletType, Queue<BaseBullet>> Pools { get; private set; } = new Dictionary<BulletType, Queue<BaseBullet>>();

        // Dictionary to remember the default size limit for the shrinking logic
        // Not just a float because it will hold references to multiple pools using the types as a label
        public Dictionary<BulletType, int> PoolLimits { get; private set; } = new Dictionary<BulletType, int>();

        private void Awake() => InitializePools();

        /// <summary>
        /// Instantiates and stores all bullets in all pools
        /// </summary>
        private void InitializePools()
        {
            //iterates through the pools defined
            foreach (var def in m_PoolSetup)
            {
                Queue<BaseBullet> newQueue = new Queue<BaseBullet>();

                //Iterates through the default size to initialise the bullets 
                for (int i = 0; i < def.DefaultSize; ++i)
                {
                    //Instantiate
                    BaseBullet b = CreateBullet(def.Prefab);
                    //add to pool
                    newQueue.Enqueue(b);
                }

                //Once created - Saves the entire pool to the pool dict
                Pools.Add(def.Type, newQueue);
                PoolLimits.Add(def.Type, def.DefaultSize);
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Instantiates the physical bullet and sets it to inactive
        /// </summary>
        /// <param name="prefab">The bullet to be instantiated</param>
        /// <returns></returns>
        private BaseBullet CreateBullet(BaseBullet prefab)
        {
            //spawns the bullet in the world as inactive
            BaseBullet b = Instantiate(prefab, transform);
            b.gameObject.SetActive(false);
            if(!b) Debug.LogError("AmmoPool: Bullet instantiation failed!");
            return b;
        }

        /// <summary>
        /// Removes the bullet from the relevant object pool and activates it
        /// </summary>
        /// <param name="type">The type of bullet to be retrieved</param>
        /// <returns></returns>
        public BaseBullet GetBullet(BulletType type)
        {
            // Won't run if no pools exist
            if (!Pools.ContainsKey(type)) return null;

            Queue<BaseBullet> queue = Pools[type];
            
            // If there are bullets in the pool 
            if (queue.Count > 0)
            {
                //remove from the pool
                BaseBullet b = queue.Dequeue();
                //activate for use
                b.gameObject.SetActive(true);
                return b;
            }

            //If the pool is empty - dynamically expand
            //Finds pool in the pool list which matches the type param
            var def = m_PoolSetup.Find(x => x.Type == type);
            //Creates a singular bullet on the fly
            //Todo - Temporarily expand the size of the pool by 15% instead
            //Maybe by comparing the current size to the default size
            //If the extended size isn't used for a set amount of mags - reset back to default
            //This would have to be checked whenever any bullets are returned - if size >= default then usedExcess=true else usedExcess=false; notUsedExcess++; 
            BaseBullet newBullet = CreateBullet(def.Prefab);
            newBullet.gameObject.SetActive(true);
            //Given directly to the player
            return newBullet;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Stores bullets - destroys any excess
        /// </summary>
        /// <param name="bullet">Bullet obj to be stored</param>
        /// <param name="type">Type of bullet to be stored</param>
        public void ReturnBullet(BaseBullet bullet, BulletType type)
        {
            //Finds the correct queue and capacity for that bullet type
            Queue<BaseBullet> queue = Pools[type];
            int limit = PoolLimits[type];

            //Destroys any bullets being returned which exceed the limit
            if (queue.Count >= limit)
            {
                Destroy(bullet.gameObject);
            }
            else
            {
                if (bullet.gameObject.activeSelf)
                {
                    bullet.gameObject.SetActive(false);
                }
                //Stores any which don't exceed the limit
                queue.Enqueue(bullet);
            }
        }
    }
}