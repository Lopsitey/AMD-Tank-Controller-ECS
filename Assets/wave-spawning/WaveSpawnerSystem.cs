using ECS.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ECS.wave_spawning
{
    /// <summary>
    /// The wave spawner system, which uses wave spawner data
    /// grabbed from SpawnerDataSingleton.
    /// </summary>
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

        public void OnCreate(ref SystemState state)
        {
            //Run only when we have data
            state.RequireForUpdate<SpawnerDataSingleton>();

            //Setup default state
            timer = 0.0f;
            waveIndex = 0;
            waveRuleIndex = 0;
            lastWaveSpawnTime = 0.0f;
            hasFinished = false;
            //A query which can be used to check the amount of enemies in the world
            enemyQuery = state.GetEntityQuery((typeof(EnemySpawnerComponent)));
        }

        /// <summary>
        /// Updates the wave spawner. Contains the main logic of this system.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="waveSpawnerData"></param>
        private void UpdateWaveSpawner(ref SystemState state, in SpawnerDataSingleton waveSpawnerData)
        {
            //Relevant data for use in this function
            var waves = waveSpawnerData.waves.GetKeyValueArrays(Allocator.Temp);
            float timeSinceSpawn = GetTimeSinceLastSpawn(ref state);
            var currentWaveData = waves.Values[waves.Keys[waveIndex]];
            
            var currentRule = currentWaveData.waveRules[waveRuleIndex];
            
            //Whether a population cap or timer has been specified
            bool specifiedPopCap = currentRule.triggerPopulationCap >= 0;
            bool specifiedTimer = currentRule.triggerTimeSinceLastSpawn >= 0;
            
            //Checks whether either has been exceeded, triggering the next wave rule
            bool metPopThreshold = enemyCount <= currentRule.triggerPopulationCap;
            bool timerExceeded = timer < currentRule.triggerTimeSinceLastSpawn;
            
            if((specifiedPopCap && metPopThreshold) || (specifiedTimer && timerExceeded))
            {
                waveRuleIndex++;
                lastWaveSpawnTime = (float)SystemAPI.Time.ElapsedTime;//Now
                timer = 0.0f;

                //Gets all units and clusters
                var units = waveSpawnerData.units;
                var clusters = waveSpawnerData.clusters;
            
                var currentUnit = units[waveRuleIndex];
                var currentCluster = clusters[waveRuleIndex];
                
                if (currentRule.type == WaveRuleType.Cluster)
                {
                    foreach (var rule in currentCluster.clusterRules)
                    {
                        Debug.Log($"Spawning {rule.amount} of unit {currentUnit.name} from cluster {currentCluster.name} as part of wave {currentWaveData.name}");
                        for (int i = 0; i < rule.amount; i++)
                        {
                            Debug.Log($"Spawning unit {i} in the cluster.");
                            //Entity enemy = ecb.Instantiate(currentUnit);
                                
                            //float3 pos = new float3(0f, 0f, 0f);

                            //ecb.SetComponent(enemy, LocalTransform.FromPosition(pos));
                        }
                    }
                }
                else if (currentRule.type == WaveRuleType.Unit)
                {
                    Debug.Log($"Spawning individual unit {currentUnit.name} as part of wave {currentWaveData.name}");
                    //Entity enemy = ecb.Instantiate(currentUnit);
                                
                    //float3 pos = new float3(0f, 0f, 0f);

                    //ecb.SetComponent(enemy, LocalTransform.FromPosition(pos));
                }
                
                if (timeSinceSpawn < currentRule.triggerTimeSinceLastSpawn)
                    Debug.Log($"Wave rule triggered by timer! Time since last spawn: {timeSinceSpawn}, required time: {currentRule.triggerTimeSinceLastSpawn}");
                else if (metPopThreshold)
                    Debug.Log($"Wave rule triggered by population cap! Current population: {enemyCount}, required population: {currentRule.triggerPopulationCap}");
            }
            
            //If completed all rules for this wave, move to the next wave
            if (CompletedWaveRules(currentWaveData))
            {
                waveRuleIndex = 0;
                waveIndex++;
            }

            //If completed all waves, set finished to true to skip logic in future updates
            if (CompletedWaves(waves))
            {
                waveIndex = 0;
                waveRuleIndex = 0;
                hasFinished = true;
                
                Debug.Log("Completed all waves!");
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            //Finished all waves? Skip logic
            if(hasFinished)
                return;
        
            //Grab ECB & wave spawner data
            this.ecb = GetECB(ref state);
            var waveSpawner = SystemAPI.GetSingleton<SpawnerDataSingleton>();

            //Update with this wave spawner
            UpdateWaveSpawner(ref state, waveSpawner);

            //Increase timer
            timer += SystemAPI.Time.DeltaTime;
            
            enemyCount = GetEntityCount(ref state, enemyQuery);
            Debug.Log($"There are currently {enemyCount} enemies in the world.");
            
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


        #endregion
    }
}
