using Unity.Entities;
using Unity.Mathematics;
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
}