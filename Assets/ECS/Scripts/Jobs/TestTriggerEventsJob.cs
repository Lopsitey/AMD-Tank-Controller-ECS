using Unity.Burst;
using Unity.Collections;
using Unity.Physics;
using UnityEngine;

namespace ECS.Scripts.Jobs
{
    [BurstCompile]
    public partial struct TestTriggerEventsJob : ITriggerEventsJob
    {
        [ReadOnly] public NativeArray<RigidBody> m_Bodies;

        public const byte SPHERE_TAG_MASK = (1 << 0);// 0b01 = 1
        public const byte TRIGGER_VOLUME_TAG_MASK = (1 << 1);// 0b10 = 2

        // Checks if the given body has the specified tag by performing a bitwise AND operation and checking if the result is greater than 0
        // Essentially, it checks if the bits corresponding to the tag are set in the body's CustomTags field
        public static bool HasBodyTag(in RigidBody body, in byte tag) 
            => (body.CustomTags & tag) > 0;

        // Checks if either body has exclusively the specified tag and the other body has the other tag, allowing for both possible combinations
        public static bool DoesBodyTagPairMatch(in RigidBody a, in byte aMask, in RigidBody b, in byte bMask)
        {
            bool aMatch = HasBodyTag(a, aMask) && HasBodyTag(b, bMask);
            bool bMatch = HasBodyTag(a, bMask) && HasBodyTag(b, aMask);
            return aMatch || bMatch;
        }
        public void Execute(TriggerEvent triggerEvent)
        {
            RigidBody a = m_Bodies[triggerEvent.BodyIndexA];
            RigidBody b = m_Bodies[triggerEvent.BodyIndexB];

            if (DoesBodyTagPairMatch(a, SPHERE_TAG_MASK, b, TRIGGER_VOLUME_TAG_MASK))
                Debug.Log("The sphere entered the trigger volume!");
        }
    }
}