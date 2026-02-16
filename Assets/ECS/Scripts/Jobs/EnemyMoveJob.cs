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
    public partial struct EnemyMoveJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Entity playerEntity;
        [ReadOnly] public LocalTransform playerLT;

        public void Execute(in EnemyComponent enemyComp, ref LocalTransform enemyLT, ref PhysicsVelocity velocity, in LocalToWorld enemyL2W)
        {
            // Gets the player position
            float3 playerPos = playerLT.Position;
                
            // Gets the direction to the enemy and then uses it to calculate the point to move towards
            float3 enemyDir = math.normalizesafe(playerPos - enemyL2W.Position);
            float3 targetVelocity = enemyDir * enemyComp.m_MoveSpeed;
            
            // Get Current Horizontal Velocity ignoring the "y" component so gravity stays the same
            float3 currentHorizontalVel = new float3(velocity.Linear.x, 0, velocity.Linear.z);
            // Smoothly interpolates from the current to the target velocity, with 10 being the rate of the change
            float3 newHorizontalVel = math.lerp(currentHorizontalVel, targetVelocity, 10 * deltaTime);
            
            // Applies the smoothed velocity (also doesn't change "y" axis)
            velocity.Linear = new float3(newHorizontalVel.x, velocity.Linear.y, newHorizontalVel.z);
            
            // Return early if not moving to avoid rotation on the spot
            if (newHorizontalVel.Equals(float3.zero)) return;
            
            // Slerp (Spherical Lerp) towards the player
            quaternion targetRot = quaternion.LookRotationSafe(enemyDir, math.up());
            enemyLT.Rotation = math.slerp(enemyLT.Rotation, targetRot, 10f * deltaTime);
        }
    }
}