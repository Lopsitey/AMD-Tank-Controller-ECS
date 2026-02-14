using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
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
            
            //Make enemy update job and schedule it here...
        }
    }
}