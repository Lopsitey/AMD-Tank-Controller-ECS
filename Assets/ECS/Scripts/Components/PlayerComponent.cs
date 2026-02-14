using Unity.Entities;

namespace ECS.Scripts.Components
{
    public struct PlayerComponent : IComponentData
    {
        public float moveSpeed;
    }
}