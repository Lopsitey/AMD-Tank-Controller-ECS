using Unity.Entities;

namespace ECS.Scripts.Components
{
    // Inherits from IBufferElementData, which means it can be used as a buffer component.
    // Can store multiple damage events for an entity.
    public struct DamageBufferComponent : IBufferElementData
    {
        public float m_Damage;
        public Entity m_Causer;
    }
}