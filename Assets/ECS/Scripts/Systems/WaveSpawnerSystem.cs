using ECS.Scripts.Components;
using ECS.wave_spawning;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace wave_spawning
{
    /// <summary>
    /// The wave spawner system, which uses wave spawner data
    /// grabbed from SpawnerDataSingleton.
    /// </summary>
    [BurstCompile]
    public partial struct WaveSpawnerSystem : ISystem
    {
        private float timer;
        private int waveIndex;
        private int waveRuleIndex;
        private float lastWaveSpawnTime;

        private bool hasFinished;
        private EntityCommandBuffer ecb;
        private EntityQuery enemyQuery;
        private int enemyCount;

        private int clusterRuleIndex;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaveSpawnerDataAuthoring.UnitPrefabRegistryTag>();
            state.RequireForUpdate<EnemySpawnerComponent>();
            //Run only when data exists
            state.RequireForUpdate<SpawnerDataSingleton>();

            //Setup default state
            timer = 0.0f;
            waveIndex = 0;
            waveRuleIndex = 0;
            lastWaveSpawnTime = 0.0f;
            hasFinished = false;
            clusterRuleIndex = 0;
            //A query which can be used to check the amount of enemies in the world
            enemyQuery = state.GetEntityQuery((typeof(EnemyComponent)));
        }

        /// <summary>
        /// Updates the wave spawner. Contains the main logic of this system.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="waveSpawnerData"></param>
        private void UpdateWaveSpawner(ref SystemState state, in SpawnerDataSingleton waveSpawnerData)
        {
            Entity spawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerComponent>();
            EnemySpawnerComponent spawnerComp = SystemAPI.GetComponent<EnemySpawnerComponent>(spawnerEntity);

            // Ensures the spawner only iterates through one rule at a time
            if (spawnerComp.spawnedCount < spawnerComp.totalToSpawn) return;

            //Relevant data for use in this function
            var waves = waveSpawnerData.waves.GetKeyValueArrays(Allocator.Temp);
            float timeSinceSpawn = GetTimeSinceLastSpawn(ref state);

            // If completed all waves, set finished to true to skip logic in future updates
            if (CompletedWaves(waves))
            {
                waveIndex = 0;
                waveRuleIndex = 0;

                // Return early to avoid trying to access wave data that doesn't exist
                if (hasFinished) return;

                Debug.Log($"Completed all waves with {enemyCount} enemies left in the world.!");
                hasFinished = true;
                return;
            }

            int index = waves.Keys[waveIndex];
            var currentWaveData = waveSpawnerData.waves[index];

            // If all rules have been completed or the wave was empty, move to the next wave.
            if (CompletedWaveRules(currentWaveData))
            {
                // Move to the next wave
                waveIndex++;
                waveRuleIndex = 0; // Reset rule index for the new wave
                return;
            }

            var currentRule = currentWaveData.waveRules[waveRuleIndex];

            //Whether a population cap or timer has been specified
            bool specifiedPopCap = currentRule.triggerPopulationCap >= 0;
            bool specifiedTimer = currentRule.triggerTimeSinceLastSpawn >= 0;

            //Checks whether either has been exceeded, triggering the next wave rule
            bool metPopThreshold = enemyCount >= currentRule.triggerPopulationCap;
            bool timerExceeded = timeSinceSpawn >= currentRule.triggerTimeSinceLastSpawn;
            
            bool noConditionsSpecified = !specifiedPopCap && !specifiedTimer;
            bool shouldTrigger = 
                noConditionsSpecified ||
                (specifiedPopCap && metPopThreshold) ||
                (specifiedTimer && timerExceeded);
            
            if (shouldTrigger)
            {
                lastWaveSpawnTime = (float)SystemAPI.Time.ElapsedTime; //Now
                timer = 0.0f;
                var units = waveSpawnerData.units;

                Entity registryEntity = SystemAPI.GetSingletonEntity<WaveSpawnerDataAuthoring.UnitPrefabRegistryTag>();
                DynamicBuffer<WaveSpawnerDataAuthoring.UnitPrefabElement> prefabLookup =
                    SystemAPI.GetBuffer<WaveSpawnerDataAuthoring.UnitPrefabElement>(registryEntity);

                if (currentRule.type == WaveRuleType.Cluster)
                {
                    // clusterOrTypeId is being used here as an ID for either the unit or cluster depending on the "type" property 
                    var currentCluster = waveSpawnerData.clusters[currentRule.clusterOrTypeId];
                    Debug.Log($"Cluster: {currentCluster.name} as part of wave {currentWaveData.name}.");

                    if (CompletedClusterRules(currentCluster))
                    {
                        // Reset cluster rule index for any clusters in the next wave
                        clusterRuleIndex = 0; 
                        waveRuleIndex++;// Move to the next wave rule
                        return;
                    }
                    var clusterRule = currentCluster.clusterRules[clusterRuleIndex];
                    var currentUnit = units[clusterRule.unitId];
                    Debug.Log($"Spawning {clusterRule.amount} {currentUnit.name} in the cluster.");
                    
                    LookUpPrefab(in clusterRule.unitId, in prefabLookup, out Entity prefabToSpawn);
                    var enemyComp = new EnemyComponent
                    {
                        m_MoveSpeed = currentUnit.speed,
                        m_AttackFreq = 1.5f, // Defaults to 1.5f
                        m_AttackRange = 3f, // Defaults to 3f
                        m_AttackTimer = 0f,
                        m_MinDamage = 1,
                        m_MaxDamage = currentUnit.damage,
                        m_CurrentHealth = currentUnit.health,
                        m_MaxHealth = currentUnit.health
                    };
                    ecb.SetComponent(spawnerEntity, new EnemySpawnerComponent
                    {
                        //Didn't set the delay or position because the spawner system and job will handle that
                        entityToSpawn = prefabToSpawn,
                        timer = 0.0f,
                        spawnDelay = spawnerComp.spawnDelay,
                        name = currentUnit.name,
                        spawnedCount = 0, //Reset counter
                        totalToSpawn = clusterRule.amount,
                        enemyData = enemyComp
                    });
                    
                    clusterRuleIndex++;
                }
                else if (currentRule.type == WaveRuleType.Unit)
                {
                    var currentUnit = units[currentRule.clusterOrTypeId];
                    Debug.Log($"Spawning individual {currentUnit.name} unit as part of wave {currentWaveData.name}");
                    
                    LookUpPrefab(in currentUnit.id, in prefabLookup, out Entity prefabToSpawn);
                    var enemyComp = new EnemyComponent
                    {
                        m_MoveSpeed = currentUnit.speed,
                        m_AttackFreq = spawnerComp.enemyData.m_AttackFreq, 
                        m_AttackRange = spawnerComp.enemyData.m_AttackRange,
                        m_AttackTimer = 0f,
                        m_MinDamage = 1,
                        m_MaxDamage = currentUnit.damage,
                        m_CurrentHealth = currentUnit.health,
                        m_MaxHealth = currentUnit.health
                    };
                    ecb.SetComponent(spawnerEntity, new EnemySpawnerComponent
                    {
                        entityToSpawn = prefabToSpawn,
                        timer = 0.0f,
                        spawnDelay = spawnerComp.spawnDelay,
                        name = currentUnit.name,
                        spawnedCount = 0,
                        totalToSpawn = 1,// One unit per wave
                        enemyData = enemyComp
                    });
                    
                    waveRuleIndex++;
                }

                if (timerExceeded)
                    Debug.Log(
                        $"Wave rule triggered by timer! Time since last spawn: {timeSinceSpawn}, required time: {currentRule.triggerTimeSinceLastSpawn}");
                else if (metPopThreshold)
                    Debug.Log(
                        $"Wave rule triggered by population cap! Current population: {enemyCount}, required population: {currentRule.triggerPopulationCap}");
                else if (noConditionsSpecified)
                    Debug.Log("Wave rule triggered immediately (no conditions specified)");
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            //Finished all waves? Skip logic
            if (hasFinished)
                return;

            //Grab ECB & wave spawner data
            this.ecb = GetECB(ref state);
            var waveSpawner = SystemAPI.GetSingleton<SpawnerDataSingleton>();

            //Update with this wave spawner
            UpdateWaveSpawner(ref state, waveSpawner);

            //Increase timer
            timer += SystemAPI.Time.DeltaTime;

            enemyCount = GetEntityCount(ref state, enemyQuery);

            //And dispose of ECB
            this.ecb.Playback(state.EntityManager);
            this.ecb.Dispose();
        }

        #region Helper functions

        private EntityQuery GetEntityCountQuery<T>(ref SystemState state) where T : unmanaged, IComponentData
            => state.GetEntityQuery(typeof(T));

        private int GetEntityCount(ref SystemState state, EntityQuery query)
            => query.CalculateEntityCount();

        private float GetTimeSinceLastSpawn(ref SystemState state)
            => (float)SystemAPI.Time.ElapsedTime - lastWaveSpawnTime;

        private bool CompletedWaveRules(Wave waveData)
            => waveRuleIndex >= waveData.waveRules.Length;

        private bool CompletedClusterRules(Cluster cluster)
            => clusterRuleIndex >= cluster.clusterRules.Length;

        private bool CompletedWaves(in NativeKeyValueArrays<int, Wave> waves)
            => waveIndex >= waves.Length;

        private EntityCommandBuffer GetECB(ref SystemState state)
            => new EntityCommandBuffer(Allocator.Temp);

        /// <summary>
        /// Clears all enemies in the world. Used for debugging
        /// the population threshold condition.
        /// </summary>
        /// <param name="state"></param>
        private void ClearAllEnemies(ref SystemState state)
        {
            foreach (var (enemy, entity) in SystemAPI.Query<RefRW<EnemySpawnerComponent>>().WithEntityAccess())
                ecb.DestroyEntity(entity);
        }

        /// <summary>
        /// Finds the prefab entity for the current unit.
        /// </summary>
        /// <param name="id">The id of the prefab to find.</param>
        /// <param name="prefabLookup">The lookup table to check.</param>
        /// <param name="prefabToSpawn">The prefab output - null if none found.</param>
        private void LookUpPrefab(in int id, in DynamicBuffer<WaveSpawnerDataAuthoring.UnitPrefabElement> prefabLookup, out Entity prefabToSpawn)
        {
            prefabToSpawn = Entity.Null;
            foreach (var element in prefabLookup)
            {
                if (element.UnitID == id)
                {
                    prefabToSpawn = element.PrefabEntity;
                    // If the relevant prefab is found then stop and continue with spawning
                    break;
                }
            }
            // May not have added a prefab to the master list with that ID
            Debug.Assert(prefabToSpawn != Entity.Null,
                $"Could not find prefab for Unit ID {id} in Registry!");
        }

        #endregion
    }
}