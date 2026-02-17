using ECS.Scripts.Components;
using ECS.Scripts.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ECS.Scripts.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct EnemyUpdateDamageSystem : ISystem
    {
        private ComponentLookup<EnemyComponent> enemyLookup;
        private ComponentLookup<PlayerComponent> playerLookup;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<EnemyComponent>();

            enemyLookup = state.GetComponentLookup<EnemyComponent>();
            playerLookup = state.GetComponentLookup<PlayerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            enemyLookup.Update(ref state);
            playerLookup.Update(ref state);

            var simSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            EnemyUpdateDamageJob damageJob = new EnemyUpdateDamageJob
            {
                enemyLookup = enemyLookup,
                playerLookup = playerLookup,
                ecb = GetECB(ref state)
            };
            state.Dependency = damageJob.Schedule(simSingleton, state.Dependency);
        }
        
        private EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            return ecb;
        }
    }
}