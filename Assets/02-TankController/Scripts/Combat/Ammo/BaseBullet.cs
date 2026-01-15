using UnityEngine;

namespace _02_TankController.Scripts.Combat.Ammo
{
    public enum BulletType { Basic, FMJ, Double }

    //Abstract means it cannot be put on a prefab or instantiated directly, it must be inherited form
    [RequireComponent(typeof(Rigidbody))]
    public abstract class BaseBullet : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField] protected float m_Speed = 50f;
        [SerializeField] protected float m_Damage = 10f;
        [SerializeField] protected float m_MaxLifetime = 5f;
        
        protected Rigidbody m_Rb;
        protected float m_LifeTimer;
        
        public virtual void Awake()//todo, can this be made pure virtual?
        {
            m_Rb = GetComponent<Rigidbody>();
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

        public void Launch(Vector3 direction)
        {
            transform.forward = direction;
            // Impulse because bullets don't have a constant force, they are fired from an explosion 
            m_Rb.AddForce(direction * m_Speed, ForceMode.Impulse); 
        }

        protected virtual void DisableSelf()
        {
            gameObject.SetActive(false);
            /// The Pool will detect this disable automatically if set up correctly
        }
        
        private void OnCollisionEnter(Collision other)
        {
            // Handle damage logic here
            DisableSelf();
        }
    }
}