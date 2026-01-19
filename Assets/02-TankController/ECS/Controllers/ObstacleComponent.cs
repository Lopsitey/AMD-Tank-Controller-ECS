using Unity.Entities;
using Unity.Mathematics;

namespace _02_TankController.ECS.Controllers
{
    public struct ObstacleComponent : IComponentData
    {
        public float3 position;
        public float3 size;
    }
}
