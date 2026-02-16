using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
            var buffer = SystemAPI.GetBuffer<DamageBufferComponent>(playerEntity);

            //TODO
            //UnityEngine.Debug.Log($"The player has taken {buffer.Length} damage instances so far!");
            
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

    [BurstCompile]
    public partial struct EnemySpawnerJob : IJobEntity
    {
        public float deltaTime;
        public float elapsedTime;
        
        //Thread-safe ECB - this will be used for multithreading!
        public EntityCommandBuffer.ParallelWriter ecb;
        //Because this can run in parallel you need to pass in the index/key of the entity being processed
        
        //Execute communicates to the job which components to process similar SystemAPI.Query<RefRW<Component>>() just with different parameters
        //This function converts ref and in keywords to RefRW and RefRO wrappers automatically.
        //This finds all entities with EnemySpawnerComponent
        //The chunk index is used as the key for the ECB parallel writer
        public void Execute([ChunkIndexInQuery] int chunkIndex, ref EnemySpawnerComponent spawner, Entity entity)
        { 
            // If no more entities to spawn, skip
            if(spawner.spawnedCount >= spawner.totalToSpawn) return;
            
            spawner.timer += deltaTime;
            
            // Won't allow spawning until enough time has passed
            if (spawner.timer <= spawner.spawnDelay) return;
            
            // Calculate new position with sine wave movement
            float x = spawner.spawnPos.x + math.sin(elapsedTime * 2f) * 5f;
            float y = spawner.spawnPos.y + math.cos(elapsedTime * 2f) * 5f;
            float z = spawner.spawnPos.z + math.cos(elapsedTime * 2f) * 5f;
            
            //Builds a LocalTransform component which holds position, rotation, and scale data
            LocalTransform lt = LocalTransform.FromPosition(spawner.spawnPos);
            
            //sets the position of the spawner to a new position
            lt.Position = new float3(x, y, z);
            
            // Spawn a new entity using the chunk-index as the key for the ECB
            // Uses the entity prefab to spawn from the spawner component
            Entity spawnedEnemy = ecb.Instantiate(chunkIndex, spawner.entityToSpawn);
            
            ecb.AddComponent(chunkIndex, spawnedEnemy, new EnemyComponent
            {
                m_MoveSpeed = 1f,
                m_AttackFreq = 0.25f,
                m_AttackRange = 2,
                m_AttackTimer = 0f,
                m_MinDamage = 1f,
                m_MaxDamage = 5f
            });
            // Uses the chunk index as the key again
            // Sets the position of the spawned enemy to the spawner's spawn position
            ecb.SetComponent(chunkIndex, spawnedEnemy, lt);
            
            // Sets the name of the enemy
            ecb.SetName(chunkIndex, spawnedEnemy, $"enemy-{spawner.totalToSpawn} from {spawner.name}");
            
            //Increment the spawn count
            spawner.spawnedCount++;
            
            // Reset the timer
            spawner.timer = 0.0f;
        }
    }
}
