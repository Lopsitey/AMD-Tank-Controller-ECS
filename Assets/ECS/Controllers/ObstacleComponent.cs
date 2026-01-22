using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Controllers
{
    public struct ObstacleComponent : IComponentData
    {
        public float3 m_Position;
        public float3 m_Size;
    }
}
