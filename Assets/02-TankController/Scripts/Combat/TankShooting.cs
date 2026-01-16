#region

using System.Collections;
using _02_TankController.Scripts.Combat.Ammo;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts.Combat
{
    public class TankShooting : MonoBehaviour
    {
        [Header("Fire Point")] [SerializeField]
        private Transform m_FirePoint;

        [Header("Settings")] [SerializeField] private float m_BaseCooldown = 0.5f;

        [SerializeField] private float m_FMJCooldown = 1.5f;
        [SerializeField] private float m_DoubleBurstDelay = 0.1f; // Time between the 2 shots

        private AmmoPool m_Pool;
        private BulletType m_CurrentType = BulletType.Basic;
        private int m_TypeIndex;
        private float m_NextFireTime;

        private void Awake()
        {
            m_Pool = GetComponent<AmmoPool>();
        }

        /// <summary>
        /// Iterates through the available bullet types
        /// </summary>
        /// <param name="swapDir">1 is next -1 is previous</param>
        public void SwitchType(int swapDir)
        {
            int targetIndex = m_TypeIndex + swapDir;
            // allows wrap around
            targetIndex = targetIndex < 0 ? 2 : targetIndex > 2 ? 0 : targetIndex;
            // stored for next time
            m_TypeIndex = targetIndex;
            switch (targetIndex)
            {
                case 0:
                    m_CurrentType = BulletType.Basic;
                    break;
                case 1:
                    m_CurrentType = BulletType.FMJ;
                    break;
                case 2:
                    m_CurrentType = BulletType.Double;
                    break;
            }
            // TODO: Call UIManager.UpdateAmmoType(newType);
        }

        /// <summary>
        /// Spawns the bullet and starts the relevant cooldown
        /// </summary>
        public void Fire()
        {
            //can't fire if the cooldown is still active
            if (Time.time < m_NextFireTime)
                return;

            //gets the cooldown based on bullet type
            float cooldown = m_CurrentType == BulletType.FMJ ? m_FMJCooldown : m_BaseCooldown;
            //The exact moment it will next fire 
            m_NextFireTime = Time.time + cooldown;
            //no cooldown coroutine needed as this is more efficient

            if (m_CurrentType == BulletType.Double)
                StartCoroutine(FireDoubleBurst());
            else
                SpawnBullet();
        }

        /// <summary>
        /// Sets the position of the bullet using the fire-point and launches it
        /// </summary>
        private void SpawnBullet()
        {
            BaseBullet bullet = m_Pool.GetBullet(m_CurrentType);
            if (bullet)
            {
                bullet.transform.position = m_FirePoint.position;
                bullet.transform.rotation = m_FirePoint.rotation;
                bullet.Launch(m_FirePoint.forward);

                // IMPORTANT: We must teach the bullet how to return home
                // We can use a simple component or event, but for now:
                StartCoroutine(MonitorBullet(bullet, m_CurrentType));
            }
        }

        /// <summary>
        /// Spawns a second bullet after the burst delay
        /// </summary>
        /// <returns></returns>
        IEnumerator FireDoubleBurst()
        {
            SpawnBullet();
            yield return new WaitForSeconds(m_DoubleBurstDelay);
            SpawnBullet();
        }

        /// <summary>
        /// Calls the pool to return the bullet when it deactivates
        /// </summary>
        /// <param name="b">The bullet to return to the pool</param>
        /// <param name="type">The type of pool for the bullet to return to</param>
        /// <returns></returns>
        IEnumerator MonitorBullet(BaseBullet b, BulletType type)
        {
            //The bullet doesn't need to know about the pool because that would be tight coupling
            //That's why this helper function exists, because this manager class does know about the pool
            
            // Waits until the bullet deactivates
            while (b.gameObject.activeSelf)
            {
                yield return null;
            }

            //Sends back to the pool
            m_Pool.ReturnBullet(b, type);
        }
    }
}