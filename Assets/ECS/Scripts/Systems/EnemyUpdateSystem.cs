using ECS.Scripts.Components;
using ECS.Scripts.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ECS.Scripts.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct EnemyUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            // We need at least one EnemyComponent and the player
            state.RequireForUpdate<EnemyComponent>();
            state.RequireForUpdate<PlayerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // This assumes thee is one player - gets the entity associated with it, the player and transform comps
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            var playerLT = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var ecb = GetECB(ref state);
            
            EnemyMoveJob moveJob = new EnemyMoveJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                playerEntity = playerEntity,
                playerLT = playerLT
            };
            
            EnemyDamageJob damageJob = new EnemyDamageJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                playerEntity = playerEntity,
                playerLT = playerLT,
                playerDamageBuffer = SystemAPI.GetBuffer<DamageBufferComponent>(playerEntity),
                ecb = ecb
            };
            
            // Schedules the jobs to run in parallel across all entities with EnemyComponent
            // Starts the jobs only after the previous one have finished
            state.Dependency = moveJob.ScheduleParallel(state.Dependency);
            // Passes in the moveJob as a dependency to ensure the damage job only starts after the move job has finished
            state.Dependency = damageJob.ScheduleParallel(state.Dependency);
        }
        
        private EntityCommandBuffer.ParallelWriter GetECB(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            return ecb;
        }
    }
}