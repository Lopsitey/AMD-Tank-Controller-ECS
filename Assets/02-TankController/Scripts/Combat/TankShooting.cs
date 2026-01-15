using System.Collections;
using _02_TankController.Scripts.Combat.Ammo;
using UnityEngine;

namespace _02_TankController.Scripts.Combat
{
    public class TankShooting : MonoBehaviour
    {
        [Header("Fire Point")]
        [SerializeField] private Transform m_FirePoint;
        
        [Header("Settings")]
        [SerializeField] private float m_BaseCooldown = 0.5f;
        
        [SerializeField] private float m_FMJCooldown = 1.5f;
        [SerializeField] private float m_DoubleBurstDelay = 0.1f; // Time between the 2 shots

        private AmmoPool m_Pool;
        private BulletType m_CurrentType = BulletType.Basic;
        private int m_TypeIndex = 0;
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
        /// 
        /// </summary>
        public void Fire()
        {
            float cooldown = m_CurrentType == BulletType.FMJ ? m_FMJCooldown : m_BaseCooldown;
            m_NextFireTime = Time.time + cooldown;

            if (m_CurrentType == BulletType.Double)
            {
                StartCoroutine(FireDoubleBurst());
            }
            else
            {
                SpawnBullet();
            }
        }

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

        IEnumerator FireDoubleBurst()
        {
            SpawnBullet();
            yield return new WaitForSeconds(m_DoubleBurstDelay);
            SpawnBullet();
        }

        // A simple way to handle the return without modifying the Bullet class too much
        IEnumerator MonitorBullet(BaseBullet b, BulletType type)
        {
            // Wait until it disables itself (from lifetime or collision)
            while (b.gameObject.activeSelf)
            {
                yield return null;
            }
            // Send it back to the pool manager
            m_Pool.ReturnBullet(b, type);
        }
    }
}