using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECS.Scripts.Jobs
{
    [BurstCompile]
    public partial struct EnemyDamageJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Entity playerEntity;
        [ReadOnly] public LocalTransform playerLT;
        [ReadOnly] public DynamicBuffer<DamageBufferComponent> playerDamageBuffer;

        public EntityCommandBuffer.ParallelWriter ecb;
        
        public void Execute([ChunkIndexInQuery] int idx, ref EnemyComponent enemyComp, in LocalToWorld enemyL2W, in Entity enemyEntity)
        {
            
        }
    }
}