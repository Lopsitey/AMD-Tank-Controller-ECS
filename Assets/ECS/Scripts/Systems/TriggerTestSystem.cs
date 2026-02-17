using ECS.Scripts.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ECS.Scripts.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct TriggerTestSystem : ISystem
    {
        
        public void OnCreate(ref SystemState state)
        {   
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var simSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            // Gets all the bodies in the physics world as a NativeArray
            var allBodies = physicsWorld.CollisionWorld.Bodies;
            
            var triggerJob = new TestTriggerEventsJob
            {
                m_Bodies = allBodies
            };
            // Runs the trigger job which compares the object's tags to check if they have collided
            state.Dependency = triggerJob.Schedule(simSingleton, state.Dependency);
        }
    }
}