using ECS.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float m_MoveSpeed;
        public float m_MaxHealth;
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerComponent
                {
                    m_MoveSpeed = authoring.m_MoveSpeed,
                    m_CurrentHealth = authoring.m_MaxHealth,
                    m_MaxHealth = authoring.m_MaxHealth
                });
                AddBuffer<DamageBufferComponent>(entity);
            }
        }
    }
}