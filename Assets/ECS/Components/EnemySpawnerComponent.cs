using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace ECS.Components
{
    public struct EnemySpawnerComponent : IComponentData
    {
        public float3 spawnPos;
        public Entity entityToSpawn;
        public float timer;        
        public float spawnDelay;
        
        public FixedString32Bytes name;
        public int spawnCount;
    }
}
