using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
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
}