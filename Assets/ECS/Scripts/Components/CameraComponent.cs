using Unity.Cinemachine;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Components
{
    // Managed so it can be used easily in a managed system
    public class CameraComponent : IComponentData
    {
        // UnityObjectRef means the camera can be referenced from ECS - managed to unmanaged
        
        // A proxy transform that the camera will follow, used to get the Cinemachine to work as it likes to have a target in the real world
        public Transform m_ProxyTransform; 
        public float m_Speed;
        public CinemachineVirtualCameraBase m_VirtualCamera;
    }
    
    // A tiny component/tag which
    // Marked as ICleanup so it can get read before the entity is destroyed
    // This means the proxy can be destroyed properly when the game ends
    public struct CameraCleanupComponent : ICleanupComponentData { }
    
    // This is managed which allows it to persist after the ecs world is destroyed
    // It is added directly to the proxy
    public class CameraProxyTag : MonoBehaviour { }
}