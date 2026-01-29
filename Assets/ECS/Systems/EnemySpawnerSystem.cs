using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.Systems
{
    // Partial means this struct can be defined across multiple files
    // This is needed because the ISystem interface has extra code added by Roslyn and source generators
    public partial struct EnemySpawnerSystem : ISystem
    {
        //In Unity Entities, a type that implements `ISystem` is treated as an ECS system.
        //The Entities framework will create it and call its lifecycle methods automatically, including `OnUpdate` every frame (when enabled).
        //It runs because the Entities system discovery picks it up and schedules it in the player loop for the `World` it belongs to.
        
        // System state is passed in automatically by the ECS framework
        public void OnUpdate(ref SystemState state)
        {
            // Using means the ecb is disposed of automatically at the end of the using block
            using (var ecb = GetECB())
            {
                // This iterates over all entities with the EnemySpawnerComponent
                // RefRW is a wrapper which works like the ref keyword
                // It is faster and means you have direct r/w access to the component
                foreach (var (spawner, lt) in SystemAPI.Query<RefRW<EnemySpawnerComponent>, RefRW<LocalTransform>>())
                    UpdateSpawner(ref state, spawner, lt, ecb);
                
                //After updating playback the changes buffered in the ECB
                ecb.Playback(state.EntityManager);
            }
        }

        private void UpdateSpawner(ref SystemState state, RefRW<EnemySpawnerComponent> spawner, RefRW<LocalTransform> passedLT, EntityCommandBuffer ecb)
        {
            //Increment the timer by the time since last frame
            spawner.ValueRW.timer += SystemAPI.Time.DeltaTime;
            
            // Calculate new position with sine wave movement
            float x = spawner.ValueRO.spawnPos.x + math.sin((float)SystemAPI.Time.ElapsedTime * 2f) * 5f;
            float y = spawner.ValueRO.spawnPos.y + math.cos((float)SystemAPI.Time.ElapsedTime * 2f) * 5f;
            float z = spawner.ValueRO.spawnPos.z + math.cos((float)SystemAPI.Time.ElapsedTime * 2f) * 5f;
            
            //sets the position of the spawner to a new position
            passedLT.ValueRW.Position = new float3(x, y, z);
                            
            // If the timer is less than the spawn delay, exit early
            if (spawner.ValueRO.timer <= spawner.ValueRO.spawnDelay)
                return;
            
            // Spawn a new entity using the entity to spawn from the spawner component
            SpawnEnemy(ref state, spawner, passedLT, ecb);
            
            // Reset the timer
            spawner.ValueRW.timer = 0.0f;
        }

        private void SpawnEnemy(ref SystemState state, RefRW<EnemySpawnerComponent> spawner, RefRW<LocalTransform> passedLT, EntityCommandBuffer ecb)
        {
            // Instantiates an enemy using the entity prefab stored in the spawner component
            Entity spawnedEnemy = ecb.Instantiate(spawner.ValueRO.entityToSpawn);
            //used to be state.EntityManager.Instantiate but the ECB is better for performance
            
            // This build a LocalTransform component which holds position, rotation, and scale data
            LocalTransform lt = LocalTransform.FromPosition(passedLT.ValueRO.Position);
            
            // Sets the position of the spawned enemy to the spawner's spawn position
            ecb.SetComponent(spawnedEnemy, lt);
            // Sets the name of the enemy
            ecb.SetName(spawnedEnemy, $"enemy-{spawner.ValueRO.spawnCount} from {spawner.ValueRO.name}");
            //Increment the spawn count
            spawner.ValueRW.spawnCount++;
        }

        /// <summary>
        /// Returns a new EntityCommandBuffer with Temp allocator, meaning the data is automatically cleaned up at the end of the frame.
        /// </summary>
        /// <returns></returns>
        private EntityCommandBuffer GetECB()//ECB buffers actions for later execution - good for multithreading
        {
            // This is unmanaged memory. To manually manage the data use Allocator.Persistent or Allocator.TempJob to make the data persist for longer.
            return new EntityCommandBuffer(Allocator.Temp);
            // This is a single-threaded ECB. For multithreaded jobs use EntityCommandBuffer.Concurrent
        }

        public partial struct ProcessArrayTestJob : IJobParallelFor
        {
            // This job type runs in parallel across multiple threads but only requires a single thread to schedule
            // The amount of times this job runs is determined when scheduling
            
            public NativeArray<int> array;
            
            // Here the index param is the current index being processed in the array
            public void Execute(int index)
            {
                //Sets the value at the current index to index * 20
                array[index] = index * 20;
            }
        }
        
        public partial struct ProcessArrayTestSystem : ISystem
        {
            public void OnUpdate(ref SystemState state)
            {
                using NativeArray<int> testArray = new NativeArray<int>(10, Allocator.TempJob);
                // Instantiating the above job using the struct data
                ProcessArrayTestJob job = new ProcessArrayTestJob
                {
                    array = testArray
                };
                
                //Just using Schedule runs the job on a single thread however, this job runs in parallel once scheduled
                JobHandle jobHandle = job.Schedule(10,2);
                //This means the job will run 10 times with 2 iterations (items processed) per thread/batch
                
                // Assigning to the system dependency chain means other jobs that depend on this one will wait for it to finish
                // Essentially tells the system: “any later scheduled work that uses the ECS safety/dependency chain and touches the same component data must wait for this handle when needed”.
                state.Dependency = jobHandle;
                
                // Immediately halts the OnUpdate on this thread until the job is done
                jobHandle.Complete();
                
                for (int i = 0; i < testArray.Length; i++)
                    UnityEngine.Debug.Log($"Index {i} has value {testArray[i]}");
            }
        }

        public partial struct ProcessArrayTestJob : IJobParallelFor
        {
            // This job type runs in parallel across multiple threads but only requires a single thread to schedule
            // The amount of times this job runs is determined when scheduling
            
            public NativeArray<int> array;
            
            // Here the index param is the current index being processed in the array
            public void Execute(int index)
            {
                //Sets the value at the current index to index * 20
                array[index] = index * 20;
            }
        }
        
        public partial struct ProcessArrayTestSystem : ISystem
        {
            public void OnUpdate(ref SystemState state)
            {
                using NativeArray<int> testArray = new NativeArray<int>(10, Allocator.TempJob);
                // Instantiating the above job using the struct data
                ProcessArrayTestJob job = new ProcessArrayTestJob
                {
                    array = testArray
                };
                
                //Just using Schedule runs the job on a single thread however, this job runs in parallel once scheduled
                JobHandle jobHandle = job.Schedule(10,2);
                //This means the job will run 10 times with 2 iterations (items processed) per thread/batch
                
                // Assigning to the system dependency chain means other jobs that depend on this one will wait for it to finish
                // Essentially tells the system: “any later scheduled work that uses the ECS safety/dependency chain and touches the same component data must wait for this handle when needed”.
                state.Dependency = jobHandle;
                
                // Immediately halts the OnUpdate on this thread until the job is done
                jobHandle.Complete();
                
                for (int i = 0; i < testArray.Length; i++)
                    UnityEngine.Debug.Log($"Index {i} has value {testArray[i]}");
            }
        }
    }
}
