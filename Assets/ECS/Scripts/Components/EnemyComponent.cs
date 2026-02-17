using Unity.Entities;

namespace ECS.Scripts.Components
{
    public struct EnemyComponent : IComponentData
    {
        public float m_MoveSpeed;
        public float m_MaxDamage;
        public float m_MinDamage;
        public float m_AttackRange;
        public float m_AttackTimer;
        public float m_AttackFreq;
        public float m_CurrentHealth;
        public float m_MaxHealth;
    }
}
