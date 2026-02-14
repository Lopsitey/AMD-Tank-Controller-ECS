using ECS.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float m_moveSpeed;
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerComponent
                {
                    moveSpeed = authoring.m_moveSpeed
                });
                AddBuffer<DamageBufferComponent>(entity);
            }
        }
    }
}