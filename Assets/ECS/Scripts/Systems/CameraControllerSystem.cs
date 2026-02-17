using ECS.Scripts.Components;
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
        protected override void OnUpdate()
        {
            // Ensures jobs from the prior frame are completed
            // SystemBase's version of state.dependency.complete
            Dependency.Complete();

            LocalTransform playerLT = default;
            bool playerExists = SystemAPI.TryGetSingletonEntity<PlayerComponent>(out var playerEntity);
            if (playerExists)
            {
                // Get a lookup for LocalTransform so the player's position can be set.
                ComponentLookup<LocalTransform>
                    lookup = SystemAPI.GetComponentLookup<LocalTransform>(true); // True means read-only
                    
                playerLT = lookup[playerEntity];
            }

            var ecb = GetECB();
            float dt = SystemAPI.Time.DeltaTime;

            // Iterates through each camera - every camera follows the player at the moment - could potentially be changed later
            foreach (var (camComp, camEntity) in SystemAPI.Query<CameraComponent>().WithEntityAccess())
            {
                bool hasCleanupTag = SystemAPI.HasComponent<CameraCleanupComponent>(camEntity);
                // If the LT was destroyed but not the cleanup tag
                bool needsCleanup =!SystemAPI.HasComponent<LocalTransform>(camEntity) && hasCleanupTag;
                if (needsCleanup)
                {
                    // Destroy the proxy if it exists
                    if (camComp.m_ProxyTransform != null)
                    {
                        Object.Destroy(camComp.m_ProxyTransform.gameObject);
                        camComp.m_ProxyTransform = null;
                    }
                    Debug.LogError("Destroying camera component because player doesn't exist");
                    
                    // Removes the component manually
                    // This is because it will stay in memory otherwise, as it's using the ICleanupComponentData interface
                    ecb.RemoveComponent<CameraComponent>(camEntity);
                    ecb.RemoveComponent<CameraCleanupComponent>(camEntity);

                    // Skip the rest of the loop for this camera since it's being destroyed
                    continue;
                }

                // Create a new empty GameObject if none exist
                if (!hasCleanupTag)
                {
                    // Add the clean-up component to the camera entity so the proxies existance can be checked
                    // This is good for cleaning up the proxy in normal use like transitions between scenes
                    ecb.AddComponent<CameraCleanupComponent>(camEntity);
                    
                    // This is created in the base scene by default so it needs to be moved 
                    GameObject newProxy = new GameObject("CameraTarget_Proxy");
                    
                    // Ensures unity recognises the object as temporary and doesn't save it in the editor
                    newProxy.hideFlags = HideFlags.DontSaveInEditor;
                    
                    // This is managed and added directly to the proxy itself as opposed to the camera
                    // This it to persist after the ECS world is destroyed
                    // This is good for cleaning up the proxy in edge cases like stopping the editor
                    newProxy.AddComponent<CameraProxyTag>();
                    
                    // Moves the proxy to the subscene of the camera
                    // Now the camera is following/referencing the object in the same scene
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(newProxy,
                        camComp.m_VirtualCamera.gameObject.scene);
                    
                    // Saves it to the component
                    camComp.m_ProxyTransform = newProxy.transform;
                    // Sets the actual CM cam to point at the new GameObject
                    camComp.m_VirtualCamera.Follow = camComp.m_ProxyTransform;
                }

                // Return early and don't apply movement if the player doens't exist
                if(!playerExists) continue;
                // The fake target for the Cinemachine camera to follow
                Transform proxy = camComp.m_ProxyTransform;
                float3 currentPos = proxy.position;
                // Get the player's position.
                float3 targetPos = playerLT.Position;

                // Smoothly move the target to the player's position
                proxy.position = math.lerp(currentPos, targetPos, camComp.m_Speed * dt);
            }
        }

        /// <summary>
        /// Defensive programming here to ensure everything runs smoothly even when the system is destroyed for a weird reason.
        /// </summary>
        protected override void OnDestroy()
        {
            // Loop through all cameras and destroy their proxies to avoid leaving orphaned GameObjects in the scene when the system is destroyed
            foreach (var camComp in SystemAPI.Query<CameraComponent>())
            {
                if (camComp.m_ProxyTransform != null) 
                    Object.Destroy(camComp.m_ProxyTransform.gameObject);
            }
            // Finds and destroys any proxies that leaked because the Entity World was destroyed first
            foreach (var proxy in Object.FindObjectsByType<CameraProxyTag>(FindObjectsSortMode.None))
            {
                if (proxy)
                    Object.Destroy(proxy.gameObject);
            }
        }

        private EntityCommandBuffer GetECB() 
            => SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);
        // Had to pass world.unmanaged here because the presentation system group is in an unmanaged world
    }
}