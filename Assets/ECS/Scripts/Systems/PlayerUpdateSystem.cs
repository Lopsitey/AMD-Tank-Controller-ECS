using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Scripts.Systems
{
    [BurstCompile]
    // Unmanaged system which moves the player based on the input component and the player component.
    public partial struct PlayerUpdateSystem : ISystem
    {
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            // Run only when we have a player and input component.
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<InputComponent>();
        }

        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            InputComponent input = SystemAPI.GetSingleton<InputComponent>();
            foreach(var (player, transform, entity) in SystemAPI.Query<RefRW<PlayerComponent>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                // Gets the move vector as a float3 and cancels y so it moves only xz
                // The .xyy swizzle is used to convert the 2D move direction into a 3D move vector, with y set to 0. 
                float3 moveVec = input.playerMoveDirection.xyy;
                moveVec.y = 0; 
                
                // Distance = speed * time
                float moveDist = player.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;
                
                // Gets the new position by adding the movement vector multiplied by the movement amount to the current position
                float3 newPos = transform.ValueRO.Position + moveVec * moveDist;
                
                // Build a new transform with the pos
                LocalTransform newLT = LocalTransform.FromPosition(newPos);
                state.EntityManager.SetComponentData(entity, newLT);
            }
        }
    }
}