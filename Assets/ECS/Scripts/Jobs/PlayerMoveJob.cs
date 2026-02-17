using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ECS.Scripts.Jobs
{
    [BurstCompile]
    public partial struct PlayerMoveJob : IJobEntity
    {
        // These members shouldn't change during the job so may as well be marked as readonly for good practice
        [ReadOnly] public float deltaTime;
        [ReadOnly] public InputComponent input;

        public void Execute(in PlayerComponent playerComp, ref LocalTransform playerLT, ref PhysicsVelocity velocity)
        {
            // If the player is stopped, don't apply movement or jump input
            if(playerComp.m_IsStopped) return;
            
            // Gets the move vector as a float3 and cancels y so it moves only xz
            // The .xyy swizzle is used to convert the 2D move direction into a 3D move vector, with y set to 0. 
            float3 moveDir = input.playerMoveDirection.xyy;
            moveDir.y = 0; 
                
            float3 targetVelocity = moveDir * playerComp.m_MoveSpeed;
            
            // Get Current Horizontal Velocity ignoring the "y" component so gravity stays the same
            float3 currentHorizontalVel = new float3(velocity.Linear.x, 0, velocity.Linear.z);
            // Smoothly interpolates from the current to the target velocity, with 10 being the rate of the change
            float3 newHorizontalVel = math.lerp(currentHorizontalVel, targetVelocity, 10 * deltaTime);
            
            // Applies 5 y velocity when the jump button is pressed
            float jumpHeight = input.jumpPressed ? 5f : 0;
            
            // Applies the smoothed velocity
            velocity.Linear = new float3(newHorizontalVel.x, velocity.Linear.y + jumpHeight, newHorizontalVel.z);

            
        }
    }
}