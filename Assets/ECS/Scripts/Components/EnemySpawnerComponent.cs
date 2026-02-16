using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Scripts.Components
{
    public struct EnemySpawnerComponent : IComponentData
    {
        public float3 spawnPos;
        public Entity entityToSpawn;
        public float timer;        
        public float spawnDelay;
        
        public FixedString32Bytes name;
        public int spawnedCount;
        public int totalToSpawn;

        public EnemySpawnerComponent(Entity entityToSpawn, float spawnDelay, float3 spawnPos, float timer, FixedString32Bytes name, int totalToSpawn, int spawnedCount)
        {
            this.entityToSpawn = entityToSpawn;
            this.spawnDelay = spawnDelay;
            this.spawnPos = spawnPos;
            this.timer = timer;
            this.name = name;
            this.spawnedCount = spawnedCount;
            this.totalToSpawn = totalToSpawn;
        }
    }
}
