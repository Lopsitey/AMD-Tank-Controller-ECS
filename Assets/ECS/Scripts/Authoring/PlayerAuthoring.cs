using ECS.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float m_MoveSpeed;
        public float m_Health;
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerComponent
                {
                    m_MoveSpeed = authoring.m_MoveSpeed,
                    m_Health = authoring.m_Health
                });
                AddBuffer<DamageBufferComponent>(entity);
            }
        }
    }
}