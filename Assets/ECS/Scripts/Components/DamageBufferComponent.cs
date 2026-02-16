using Unity.Entities;

namespace ECS.Scripts.Components
{
    // Inherits from IBufferElementData, which means it can be used as a buffer component.
    // Can store multiple damage events for an entity.
    // Sets the default capacity of the buffer to 100 elements. This is just an optimization to avoid resizing the buffer too often.
    [InternalBufferCapacity(100)]
    public struct DamageBufferComponent : IBufferElementData
    {
        public float m_Damage;
        public Entity m_Causer;
    }
}