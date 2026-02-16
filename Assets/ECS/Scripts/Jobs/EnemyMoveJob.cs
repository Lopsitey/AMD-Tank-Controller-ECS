using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Scripts.Jobs
{
    [BurstCompile]
    public partial struct EnemyMoveJob : IJobEntity
    {
        // These members shouldn't change during the job so may as well be marked as readonly for good practice
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Entity playerEntity;
        [ReadOnly] public LocalTransform playerLT;

        public void Execute(in EnemyComponent enemyComp, ref LocalTransform enemyLT, in LocalToWorld enemyL2W)
        {
            // Gets the player position
            float3 playerPos = playerLT.Position;
                
            // Gets the direction to the enemy and then uses it to calculate the point to move towards
            float3 enemyDir = math.normalizesafe(playerPos - enemyL2W.Position);
            float3 steeringVec = enemyDir * deltaTime * enemyComp.m_MoveSpeed;
                
            // Move enemy towards target
            enemyLT.Position += steeringVec;
        }
    }
}