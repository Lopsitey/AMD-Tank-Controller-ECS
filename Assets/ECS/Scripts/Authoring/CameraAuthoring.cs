using ECS.Scripts.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public struct CameraComponent : IComponentData
    {
        // UnityObjectRef here means the camera can be referenced from ECS
        // This could be anything that inherits from UnityEngine.Object
        public UnityObjectRef<Camera> m_Camera;
        public float3 m_Offset;
        public float m_Speed;
    }
    
    public class CameraAuthoring : MonoBehaviour
    {
        public Vector3 offset;
        public float speed;
        private class CameraBaker : Baker<CameraAuthoring>
        {
            public override void Bake(CameraAuthoring authoring)
            {
                // Gets the entity
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Add a camera component
                AddComponent(entity, new CameraComponent
                {
                    m_Camera = GetComponent<Camera>(),
                    m_Offset = authoring.offset,
                    m_Speed = authoring.speed
                });
            }
        }
    }
    
    // Forces the camera to update after physics so it doesn't clash with the physics system.
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct CameraControllerSystem : ISystem
    {
        public void OnCreate(ref SystemState state) => state.RequireForUpdate<PlayerComponent>();

        public void OnUpdate(ref SystemState state)
        {
            // Find the player and its associated entity
            PlayerComponent player = SystemAPI.GetSingleton<PlayerComponent>();
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            
            // Get a lookup for LocalTransform so the player's position can be set.
            ComponentLookup<LocalTransform> lookup = SystemAPI.GetComponentLookup<LocalTransform>(true);// True means read-only

            foreach (var cameraInst in SystemAPI.Query<RefRW<CameraComponent>>())
            {
                // Iterates through each camera
                // TODO use singletons here or specify what should be followed per-camera
                
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