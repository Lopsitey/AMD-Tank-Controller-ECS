using ECS.Scripts.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Scripts.Jobs
{
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
            if (spawner.spawnedCount >= spawner.totalToSpawn) return;

            spawner.timer += deltaTime;

            // Won't allow spawning until enough time has passed
            if (spawner.timer <= spawner.spawnDelay) return;

            // Calculate new position with sine wave movement
            // Makes a horizontal elliptical circle
            float x = spawner.spawnPos.x + math.sin(elapsedTime * 2f) * 5f;
            float z = spawner.spawnPos.z + math.cos(elapsedTime * 2f) * 3f;

            //Builds a LocalTransform component which holds position, rotation, and scale data
            LocalTransform lt = LocalTransform.FromPosition(spawner.spawnPos);

            //Sets the position of the spawner to a new position
            lt.Position = new float3(x, 0, z);

            // Spawn a new entity using the chunk-index as the key for the ECB
            // Uses the entity prefab to spawn from the spawner component
            Entity spawnedEnemy = ecb.Instantiate(chunkIndex, spawner.entityToSpawn);

            // Adds the enemy data component of the spawned enemy to the data from the spawner component
            ecb.AddComponent(chunkIndex, spawnedEnemy, spawner.enemyData);
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