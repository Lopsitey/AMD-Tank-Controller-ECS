#region

using System.Collections;
using _02_TankController.Resources;
using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts.Combat.Ammo
{
    public enum BulletType
    {
        Basic,
        FMJ,
        Double
    }

    //bullets must have a rigidbody
    [RequireComponent(typeof(Rigidbody))]
    //Abstract means it cannot be put on a prefab or instantiated directly, it must be inherited form
    public abstract class BaseBullet : MonoBehaviour
    {
        [Header("Bullet Profile")] [SerializeField]
        protected BulletProfile m_BulletProfile;
        [SerializeField] protected GameObject m_ExplosionPrefab;
        [SerializeField] protected int m_ExplosionTime = 5;//how long the VFX is played for

        protected float m_Speed = 50f;
        protected float m_Damage = 10f;
        protected float m_MaxLifetime = 5f;
        protected float m_LifeTimer;

        protected Rigidbody m_Rb;

        public virtual void Awake()
        {
            //Rigidbody is required due to RequireComponent attribute
            m_Rb = GetComponent<Rigidbody>();
            if (m_BulletProfile)
            {
                m_Speed = m_BulletProfile.m_Speed;
                m_Damage = m_BulletProfile.m_Damage;
                m_MaxLifetime = m_BulletProfile.m_MaxLifetime;
                m_Rb.useGravity = m_BulletProfile.m_UseGravity;
            }
        }

        public virtual void OnEnable()
        {
            m_LifeTimer = 0;
            m_Rb.linearVelocity = Vector3.zero; // Resets physics
            m_Rb.angularVelocity = Vector3.zero;
        }

        public virtual void Update()
        {
            m_LifeTimer += Time.deltaTime;
            if (m_LifeTimer >= m_MaxLifetime)
            {
                DisableSelf();
            }
        }

        /// <summary>
        /// Moves the bullet physically in the given direction, using a set speed and an impulse force
        /// </summary>
        /// <param name="direction">The direction to move the bullet</param>
        public void Launch(Vector3 direction)
        {
            transform.forward = direction;
            // Impulse because bullets don't have a constant force, they are fired from an explosion 
            m_Rb.AddForce(direction * m_Speed, ForceMode.Impulse);
        }

        //Doesn't return itself to the pool because that is the manager's job
        protected virtual void DisableSelf()
        {
            gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision other)
        {
            Instantiate(m_ExplosionPrefab, transform.position, Quaternion.identity);
            CinemachineImpulseSource explosionSource = GetComponentInChildren<CinemachineImpulseSource>();
            if(explosionSource)
                explosionSource.GenerateImpulse();
            DisableSelf();
        }
    }
}