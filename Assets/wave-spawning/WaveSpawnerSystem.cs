using ECS.Components;
using Unity.Collections;
using Unity.Entities;

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

            //TODO: Implement wave spawner
            //..       
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
