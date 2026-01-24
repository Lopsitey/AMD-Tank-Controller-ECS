using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct EnemySpawnerComponent : IComponentData
    {
        public float3 spawnPos;
        public Entity entityToSpawn;
        public float timer;        
        public float spawnDelay;
    }
}
