using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            // This iterates over all entities with the EnemySpawnerComponent
            // RefRW is a wrapper which works like the ref keyword
            // It is faster and means you have direct r/w access to the component
            foreach (var (spawner, lt) in SystemAPI.Query<RefRW<EnemySpawnerComponent>, RefRW<LocalTransform>>())
                UpdateSpawner(ref state, spawner, lt);
        }

        private void UpdateSpawner(ref SystemState state, RefRW<EnemySpawnerComponent> spawner, RefRW<LocalTransform> passedLT)
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
            SpawnEnemy(ref state, spawner, passedLT);
            
            // Reset the timer
            spawner.ValueRW.timer = 0.0f;
        }

        private void SpawnEnemy(ref SystemState state, RefRW<EnemySpawnerComponent> spawner, RefRW<LocalTransform> passedLT)
        {
            // Instantiates an enemy using the entity prefab stored in the spawner component
            Entity spawnedEnemy = state.EntityManager.Instantiate(spawner.ValueRO.entityToSpawn);
            
            // This build a LocalTransform component which holds position, rotation, and scale data
            LocalTransform lt = LocalTransform.FromPosition(passedLT.ValueRO.Position);
            
            // Sets the position of the spawned enemy to the spawner's spawn position
            state.EntityManager.SetComponentData(spawnedEnemy, lt);
            // Sets the name of the enemy
            state.EntityManager.SetName(spawnedEnemy, $"enemy-{spawner.ValueRO.spawnCount} from {spawner.ValueRO.name}");
            //Increment the spawn count
            spawner.ValueRW.spawnCount++;
        }
    }
}
