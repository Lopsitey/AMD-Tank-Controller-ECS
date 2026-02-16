using UnityEngine;

namespace _02_TankController.Resources
{
    [CreateAssetMenu(fileName = "BulletProfile", menuName = "Scriptable Objects/BulletProfile", order = 0)]
    public class BulletProfile : ScriptableObject
    {
        [Header("Bullet Settings")]
        [SerializeField] public float m_Speed = 50f;
        [SerializeField] public float m_Damage = 10f;
        [SerializeField] public float m_MaxLifetime = 5f;
        [SerializeField] public bool m_UseGravity = true;
    }
}