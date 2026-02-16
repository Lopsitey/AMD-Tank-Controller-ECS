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
        
        public EnemyComponent enemyData;
    }
}
