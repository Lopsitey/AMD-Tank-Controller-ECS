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
}