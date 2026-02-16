using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            enemyComp.m_AttackTimer += deltaTime;

            // If the timer is lower than the frequency, then return early
            if (enemyComp.m_AttackTimer <= enemyComp.m_AttackFreq) return;
            
            // Reset the timer
            enemyComp.m_AttackTimer = 0f;
            
            // If the enemy isn't within attack range, return early
            if (math.distance(enemyL2W.Position, playerLT.Position) > enemyComp.m_AttackRange) return;
            
            // If the buffer is full don't add more damage
            if (playerDamageBuffer.Length >= playerDamageBuffer.Capacity) return;
            
            // Otherwise add damage to the buffer
            ecb.AppendToBuffer(idx, playerEntity, new DamageBufferComponent
            {
                m_Causer = enemyEntity,
                m_Damage = enemyComp.m_MinDamage
            });
            
            // Calculate random damage between min and max
            float damage = UnityEngine.Random.Range(enemyComp.m_MinDamage, enemyComp.m_MaxDamage);
        }
    }
}