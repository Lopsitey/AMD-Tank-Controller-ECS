using ECS.Scripts.Components;
using Unity.Cinemachine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Scripts.Systems
{
    // Forces the camera to update after everything else has updated
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    // Managed system so no [BurstCompile] needed
    public partial class CameraControllerSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<PlayerComponent>();
        }

        protected override void OnUpdate()
        {
            // Ensures jobs from the prior frame are completed
            // SystemBase's version of state.dependency.complete
            Dependency.Complete();
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            
            // Get a lookup for LocalTransform so the player's position can be set.
            ComponentLookup<LocalTransform> lookup = SystemAPI.GetComponentLookup<LocalTransform>(true);// True means read-only
            // Exit early if the player wasn't found
            if (!lookup.HasComponent(playerEntity)) return;
            
            LocalTransform playerLT = lookup[playerEntity];
            float dt = SystemAPI.Time.DeltaTime;
            
            // Iterates through each camera - every camera follows the player at the moment - could potentially be changed later
            foreach (var camComp in SystemAPI.Query<CameraComponent>())
            {
                // Create a new empty GameObject if none exist
                if (!camComp.m_ProxyTransform)
                {
                    // This is created in the base scene by default so it needs to be moved 
                    GameObject newProxy = new GameObject("CameraTarget_Proxy");
                    
                    // Moves the proxy to the subscene of the camera
                    // Now the camera is following/referencing the object in the same scene
                    UnityEngine.SceneManagement.SceneManager.
                        MoveGameObjectToScene(newProxy,camComp.m_VirtualCamera.gameObject.scene);
                    
                    // Saves it to the component
                    camComp.m_ProxyTransform = newProxy.transform;
                    // Sets the actual CM cam to point at the new GameObject
                    camComp.m_VirtualCamera.Follow = camComp.m_ProxyTransform;
                }

                // The fake target for the Cinemachine camera to follow
                Transform proxy = camComp.m_ProxyTransform;
                float3 currentPos = proxy.position;
                // Get the player's position.
                float3 targetPos = playerLT.Position;
                
                // Smoothly move the target to the player's position
                proxy.position = math.lerp(currentPos, targetPos, camComp.m_Speed * dt);
                // Optional: Sync rotation if you are using "3rd Person Follow" mode
                // proxy.rotation = math.slerp(proxy.rotation, playerLT.Rotation, 15f * dt);
            }
        }
        
        protected override void OnDestroy()
        {
            // Loop through all cameras and destroy their proxies to avoid leaving orphaned GameObjects in the scene when the system is destroyed
            foreach (var camComp in SystemAPI.Query<CameraComponent>())
            {
                if (camComp.m_VirtualCamera)
                    camComp.m_VirtualCamera.Follow = null;
                if (camComp.m_ProxyTransform)
                    Object.Destroy(camComp.m_ProxyTransform.gameObject);   
            }
        }
    }
}