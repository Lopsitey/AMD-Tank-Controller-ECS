using Unity.Entities;
using Unity.Physics;

namespace ECS.Scripts.Utility
{
    /// <summary>
    /// Contains functions for dealing with physics events.
    /// Entities are set in a TriggerEvent (or similar)
    /// as <br /> ".EntityA" or ".EntityB".
    /// This helps check if these entities have a component.
    /// </summary>
    public static class PhysicsEventUtils
    {
        /// <summary>
        /// Represents an Entity and a component attached to
        /// that entity -- a pair.
        /// </summary>
        /// <typeparam name="T">The type of component</typeparam>
        public struct EntityComponentPair<T> where T : unmanaged, IComponentData
        {
            public Entity entity;
            public T component;
        }
 
        /// <summary>
        /// Represents a match returned from ComponentLookupMatch. T is the
        /// first type of component to be looked up, U is the second.
        /// </summary>
        /// <typeparam name="T">The first component to lookup via lookupA.</typeparam>
        /// <typeparam name="U">The second component to lookup via lookupB.</typeparam>
        public struct EntityComponentMatch<T, U>
            where T : unmanaged, IComponentData
            where U : unmanaged, IComponentData
        {
            /// <summary>
            /// Whether there was in fact, a match. This is true if
            /// entityA has T and entityB has U, or vice versa.
            /// </summary>
            public bool matched;
 
            /// <summary>
            /// The first pair (of type T)
            /// </summary>
            public EntityComponentPair<T> pairA;
 
            /// <summary>
            /// The second pair (of type U)
            /// </summary>
            public EntityComponentPair<U> pairB;
 
            /// <summary>
            /// Auto deconstructs this struct into a tuple.
            /// </summary>
            /// <param name="matched"></param>
            /// <param name="a"></param>
            /// <param name="b"></param>
            public void Deconstruct(out bool matched, out EntityComponentPair<T> a, out EntityComponentPair<U> b)
            {
                matched = this.matched;
                a = this.pairA;
                b = this.pairB;
            }
        }
 
        /// <summary>
        /// Performs a component lookup on both entities in the trigger event, using
        /// the two lookups provided. If there is a mutually exclusive match, that being:
        /// 
        /// - Entity A has component A, Entity B has component B; or
        /// - Entity B has component A, Entity A has component B
        /// 
        /// If either of these is true, it returns a EntityComponentMatch whose .matched
        /// member is true. Otherwise, it is false.
        /// 
        /// If a match is present, the method organises T and U such that it returns a pair which
        /// matches type T (EntityComponentPair) and another matching U.
        /// <code>
        /// Example usage:
        /// var (matched, player, enemy) = PhysicsEventUtils.ComponentLookupMatch(trigEvent, playerLookup, enemyLookup);
        /// with playerLookup = ComponentLookup&lt;PlayerComponent&gt; and enemyLookup = ComponentLookup&lt;EnemyComponent&gt;
        /// </code>
        /// </summary>
        /// <typeparam name="T">The type of the first component to lookup.</typeparam>
        /// <typeparam name="U">The type of the second component to lookup.</typeparam>
        /// <param name="trigEvent">The trigger event from your Execute method.</param>
        /// <param name="lookupA">The first lookup.</param>
        /// <param name="lookupB">The second lookup.</param>
        /// <returns>An EntityComponentMatch which represents the matched types.</returns>
        public static EntityComponentMatch<T, U> ComponentLookupMatch<T, U>(TriggerEvent trigEvent, ComponentLookup<T> lookupA, ComponentLookup<U> lookupB)
            where T : unmanaged, IComponentData
            where U : unmanaged, IComponentData
        {
            //Check if either A or B matched exclusively
            bool componentMatchA = lookupA.HasComponent(trigEvent.EntityA) && lookupB.HasComponent(trigEvent.EntityB);
            bool componentMatchB = lookupB.HasComponent(trigEvent.EntityA) && lookupA.HasComponent(trigEvent.EntityB);
 
            //No match, return out
            if (!componentMatchA && !componentMatchB)
                return new EntityComponentMatch<T, U> { matched = false };
 
            //Otherwise figure out which is T and which is U
            T valueT = (componentMatchA) ? (lookupA[trigEvent.EntityA]) : (lookupA[trigEvent.EntityB]);
            U valueU = (componentMatchB) ? (lookupB[trigEvent.EntityA]) : (lookupB[trigEvent.EntityB]);
 
            //Figure out entity based on T and U
            Entity entityT = componentMatchA ? trigEvent.EntityA : trigEvent.EntityB;
            Entity entityU = componentMatchB ? trigEvent.EntityA : trigEvent.EntityB;
 
            return new EntityComponentMatch<T, U>
            {
                matched = true,
                pairA = new EntityComponentPair<T> { component = valueT, entity = entityT },
                pairB = new EntityComponentPair<U> { component = valueU, entity = entityU }
            };
        }
    }
}