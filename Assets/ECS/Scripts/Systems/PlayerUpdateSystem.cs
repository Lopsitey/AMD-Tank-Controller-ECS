using ECS.Scripts.Components;
using ECS.Scripts.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Scripts.Systems
{
    // Ensures it updates in the fixed physics group - like FixedUpdate 
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    // Ensures it runs before the physics engine solves the frame
    // Ordered like this so the data is ready for the physics system to process it
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    // Unmanaged system which moves the player based on the input component and the player component.
    public partial struct PlayerUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Run only when we have a player and input component.
            state.RequireForUpdate<PlayerComponent>();
            state.RequireForUpdate<InputComponent>();
            // Ensure the game runs at a consistent frame rate to make movement smoother and more consistent across different hardware.
            Application.targetFrameRate = 120;
        }
        
        public void OnUpdate(ref SystemState state)
        {
            PlayerMoveJob moveJob = new PlayerMoveJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                input = SystemAPI.GetSingleton<InputComponent>()
            };
            state.Dependency = moveJob.ScheduleParallel(state.Dependency);
            
            /*
            foreach(var (player, transform, entity) in SystemAPI.Query<RefRW<PlayerComponent>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                // Gets the move vector as a float3 and cancels y so it moves only xz
                // The .xyy swizzle is used to convert the 2D move direction into a 3D move vector, with y set to 0. 
                float3 moveVec = input.playerMoveDirection.xyy;
                moveVec.y = 0; 
                
                // Distance = speed * time
                float moveDist = player.ValueRO.m_MoveSpeed * SystemAPI.Time.DeltaTime;
                
                // Gets the new position by adding the movement vector multiplied by the movement amount to the current position
                float3 newPos = transform.ValueRO.Position + moveVec * moveDist;
                
                // Build a new transform with the pos
                LocalTransform newLT = LocalTransform.FromPosition(newPos);
                state.EntityManager.SetComponentData(entity, newLT);
            }
            */
        }
    }
}