using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class CustomTagAuthoring : MonoBehaviour
    {
        public byte m_Tags;
        private class CustomTagAuthoringBaker : Baker<CustomTagAuthoring>
        {
            public override void Bake(CustomTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PhysicsCustomTags
                {
                    Value = authoring.m_Tags
                });
            }
        }
    }
}