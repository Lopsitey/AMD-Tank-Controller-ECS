using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Scripts.Components
{
    public struct InputComponent : IComponentData
    {
        public bool spacePressed;
        public float2 playerMoveDirection;
        public bool jumpPressed;
        
        public float m_JumpCooldown; // Cooldown time after a jump before it can be activated again
        public float m_JumpCooldownTimer; // Timer to track jump cooldown
    }
}