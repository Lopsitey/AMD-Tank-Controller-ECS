using ECS.Scripts.Authoring;
using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Scripts.Systems
{
    // Forces the camera to update after everything else has updated
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct CameraControllerSystem : ISystem
    {
        public void OnCreate(ref SystemState state) => state.RequireForUpdate<PlayerComponent>();

        public void OnUpdate(ref SystemState state)
        {
            // Ensures jobs from the prior frame are completed
            state.Dependency.Complete();
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            
            // Get a lookup for LocalTransform so the player's position can be set.
            ComponentLookup<LocalTransform> lookup = SystemAPI.GetComponentLookup<LocalTransform>(true);// True means read-only

            // Iterates through each camera
            foreach (var cameraInst in SystemAPI.Query<RefRW<CameraComponent>>())
            {
                // Get the player's position. Build a target position: pos + up * t - fwd * 5
                LocalTransform playerLT = lookup[playerEntity];
                float3 targetCamPos = playerLT.Position + math.up() * cameraInst.ValueRO.m_Offset.y - math.forward() * cameraInst.ValueRO.m_Offset.z;
                
                // Move the camera to be at this position and look at the player
                cameraInst.ValueRW.m_Camera.Value.transform.position = targetCamPos;
                cameraInst.ValueRW.m_Camera.Value.transform.LookAt(playerLT.Position);
            }
        }
    }
}