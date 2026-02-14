using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Scripts.Systems
{
    public partial struct EnemyUpdateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // We need at least one EnemyComponent and the player
            state.RequireForUpdate<EnemyComponent>();
            state.RequireForUpdate<PlayerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // This assumes thee is one player - gets the entity associated with it, the player and transform comps
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            var player = SystemAPI.GetComponent<PlayerComponent>(playerEntity);
            var playerLT = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            
            EnemyMoveJob moveJob = new EnemyMoveJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                playerEntity = playerEntity,
                playerLT = playerLT
            };
            
            state.Dependency = moveJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
        }

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
}