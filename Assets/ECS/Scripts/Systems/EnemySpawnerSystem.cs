using ECS.Scripts.Components;
using ECS.Scripts.Jobs;
using Unity.Burst;
using Unity.Entities;

namespace ECS.Scripts.Systems
{
    // Partial means this struct can be defined across multiple files
    // This is needed because the ISystem interface has extra code added by Roslyn and source generators
    [BurstCompile]
    public partial struct EnemySpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerComponent>();
        }
        //In Unity Entities, a type that implements `ISystem` is treated as an ECS system.
        //The Entities framework will create it and call its lifecycle methods automatically, including `OnUpdate` every frame (when enabled).
        //It runs because the Entities system discovery picks it up and schedules it in the player loop for the `World` it belongs to.

        // System state is passed in automatically by the ECS framework
        public void OnUpdate(ref SystemState state)
        {
            // Checks if the job created in the last frame has finished executing before starting a new one
            state.Dependency.Complete();
            
            // Instantiating the job using the struct data
            EnemySpawnerJob spawnerJob = new EnemySpawnerJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                elapsedTime = (float)SystemAPI.Time.ElapsedTime,
                ecb = GetECB(ref state)
            };
            
            // Schedules the job to run in parallel across all entities with EnemySpawnerComponent
            // The dependency system ensures that jobs that read/write the same data run in the correct order
            // This means the main thread waits for them to complete if necessary
            state.Dependency = spawnerJob.ScheduleParallel(state.Dependency);
        }
        
        /// <summary>
        /// Returns a new EntityCommandBuffer with Temp allocator, meaning the data is automatically cleaned up at the end of the frame.
        /// </summary>
        /// <returns></returns>
        private EntityCommandBuffer.ParallelWriter GetECB(ref SystemState state)//ECB buffers actions for later execution - good for multithreading
        {

            // Finds the pre-existing singleton ECB system
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            // Gets a parallel writer version of the ECB for multithreaded use
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            // It is important to use the ECB system's ECB so that multiple systems can add commands to the same buffer
            // This is because it comes from a singleton is a parallel writer meaning it is thread-safe
            // Playback is handled automatically by the system so no need to call Playback manually
            return ecb;
            // This is an unmanaged ECB as Unity automatically deallocates this so there is no need to dispose of it manually either
        }
    }
}
