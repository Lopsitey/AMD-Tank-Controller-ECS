using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Scripts.Components
{
    public struct InputComponent : IComponentData
    {
        public bool spacePressed;
        public float2 playerMoveDirection;
    }
}