using ECS.Scripts.Components;
using Unity.Cinemachine;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class CameraAuthoring : MonoBehaviour
    {
        public float m_Speed;
        private class CameraBaker : Baker<CameraAuthoring>
        {
            public override void Bake(CameraAuthoring authoring)
            {
                // Gets the entity
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Adds a camera component - used the object version of this for the managed system
                AddComponentObject(entity, new CameraComponent
                {
                    m_VirtualCamera = authoring.GetComponent<CinemachineVirtualCameraBase>(),
                    m_ProxyTransform = null,
                    m_Speed = authoring.m_Speed
                });
            }
        }
    }
}