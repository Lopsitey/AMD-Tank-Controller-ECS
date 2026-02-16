using ECS.Scripts.Authoring;
using Unity.Entities;

namespace ECS.Scripts.Components
{
    public struct PlayerComponent : IComponentData
    {
        public float m_MoveSpeed;
        public float m_Health;
    }
}