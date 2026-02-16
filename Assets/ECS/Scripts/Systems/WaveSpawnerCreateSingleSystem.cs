using ECS.wave_spawning;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using wave_spawning;

namespace ECS.Scripts
{
    /// <summary>
    /// This is a system which has a few main responsibilities:
    /// 
    /// - To create a SpawnerDataSingleton entity & component singleton in the world.
    /// - To copy all the data from the single baked SpawnerDataWrapped to that singleton.
    /// - To manage the allocation, disposal & deallocation of persistent native containers for this data.
    /// 
    /// This is the system which makes the singleton access to all the spawner data possible. It is the 
    /// main system which starts all the other managed -> unmanaged conversion processes.
    /// </summary>
    [BurstCompile]
    public partial struct WaveSpawnerCreateSingletonSystem : ISystem, ISystemStartStop
    {
        /// <summary>
        /// Whether to show debug messages. This is set from
        /// the debug property on WaveSpawnerDataAuthoring.
        /// </summary>
        private bool m_Debug;

        public void OnCreate(ref SystemState state)
        {
            //Wait for a baked SpawnerDataWrapper to be present before running update.
            state.RequireForUpdate<SpawnerDataWrapper>();

            //>= singleton? Throw an error, something has gone very wrong
            if(SystemAPI.HasSingleton<SpawnerDataSingleton>())
                throw new UnityException("Detected more than one instance of SpawnerDataSingleton.");
            
            //Create the singleton entity and set its component data. Note the 
            //second line here calls AllocWaveSpawner(), which allocates all the native memory it needs through its lifetime
            //SpawnerDataSingleton is the component that ends up with the unmanaged data
            Entity entity = state.EntityManager.CreateEntity(typeof(SpawnerDataSingleton));
            state.EntityManager.SetComponentData(entity, AllocWaveSpawner());
        }

        public void OnStartRunning(ref SystemState state)
        {
            //This is called when there is a SpawnerDataWrapper in the world. Therefore,
            //we can guarantee there is at least one, and there should also be a SpawnerDataSingleton as we created this in OnCreate.
            var authoringWrapper = SystemAPI.GetSingleton<SpawnerDataWrapper>();
            var spawnerSingleton = SystemAPI.GetSingletonRW<SpawnerDataSingleton>();

            // ReSharper disable once InconsistentNaming
            var waveDataSO = authoringWrapper.waveData.Value;
            
            //Allocate and convert all managed to unmanaged data using the authoring data and the spawner singleton
            SetupSpawnerFromDeferredAuthoringData(waveDataSO, ref spawnerSingleton.ValueRW);

            //Set debug mode -- everything below this is just debug information that is printed out
            //Uses the var from the authoring wrapper, which is set in the WaveSpawnerDataAuthoring script in the editor
            m_Debug = authoringWrapper.debug;

            if (!m_Debug)
                return;

            foreach (var unit in spawnerSingleton.ValueRO.units)
            {
                Debug.Log($"UNIT: key {unit.Key} and name {unit.Value.name}");
            }

            foreach (var cluster in spawnerSingleton.ValueRO.clusters)
            {
                Debug.Log($"CLUSTER: key {cluster.Key} and name {cluster.Value.name}");

                foreach (var rule in cluster.Value.clusterRules)
                    Debug.Log($"- RULE: unit id {rule.unitId} ({spawnerSingleton.ValueRO.units[rule.unitId].name}) and amount {rule.amount}");
            }

            foreach (var wave in spawnerSingleton.ValueRO.waves)
            {
                Debug.Log($"WAVE: key {wave.Key} and name {wave.Value.name}");

                foreach (var rule in wave.Value.waveRules)
                    Debug.Log($"- RULE: type {rule.type}, unit/cluster id: {rule.clusterOrTypeId}, pop {rule.triggerPopulationCap}, delay {rule.triggerTimeSinceLastSpawn}");
            }
        }


        /// <summary>
        /// Deallocates the wave spawner by calling dispose on
        /// all native containers associated with it. Should be called in OnDestroy.
        /// </summary>
        /// <param name="spawner"></param>
        private void DeallocWaveSpawner(ref SpawnerDataSingleton spawner)
        {
            if(m_Debug)   Debug.Log("Deallocating wave spawner native memory");

            //We have to run through each of these individual
            //elements and call dispose because this will trigger the cluster's
            //own dispose method -- which will dispose of the rules
            foreach (var kv in spawner.clusters)
                kv.Value.Dispose();

            //Same for the waves
            foreach (var wave in spawner.waves)
                wave.Value.Dispose();

            spawner.units.Dispose();
            spawner.clusters.Dispose();
            spawner.waves.Dispose();
        }

        private void LogDuplicateIdWarning(string type, int id, FixedString32Bytes oldName, string newName)
        {
            Debug.LogWarning($"Found duplicate {type} with id {id} ({oldName}). Can't set to new {type} '{newName}' as it would overwrite it; skipping entry.");
        }

        /// <summary>
        /// This is the primary function which copies in WaveSpawnerDataAuthoring data
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="spawner"></param>
        private void SetupSpawnerFromDeferredAuthoringData(in TomBenWaveData wrapper, ref SpawnerDataSingleton spawner)
        {
            //Convert all units
            foreach (var unit in wrapper.Units)
            {
                if (spawner.units.ContainsKey(unit.id))
                {
                    LogDuplicateIdWarning("unit", unit.id, spawner.units[unit.id].name, unit.name);
                    continue;
                }

                spawner.units[unit.id] = new Unit
                {
                    damage = unit.damage,
                    id = unit.id,
                    health = unit.health,
                    name = unit.name,
                    speed = unit.speed,
                };
            }

            //Convert all clusters
            foreach (var cluster in wrapper.Clusters)
            {
                if(spawner.clusters.ContainsKey(cluster.id))
                {
                    LogDuplicateIdWarning("cluster", cluster.id, spawner.clusters[cluster.id].name, cluster.name);
                    continue;
                }

                spawner.clusters[cluster.id] = Cluster.BuildFromAuthoring(cluster.name, cluster.clusterRules);
            }

            //Convert all waves
            foreach(var wave in wrapper.Waves)
            {
                if (spawner.waves.ContainsKey(wave.id))
                {
                    LogDuplicateIdWarning("wave", wave.id, spawner.waves[wave.id].name, wave.name);
                    continue;
                }

                spawner.waves[wave.id] = Wave.BuildFromAuthoring(wave.name, wave.waveRules);
            }

            if (m_Debug)  Debug.Log("Completed deferred baking; found SpawnerDataWrapper.");
        }
   
        /// <summary>
        /// Test method to set up the spawner with some sample data, for
        /// debug purposes.
        /// </summary>
        /// <param name="spawner">The spawner to set up</param>
        private void SetupTestSpawner(ref SpawnerDataSingleton spawner)
        {
            //Set up units
            spawner.units[0] = new Unit { damage = 25, health = 100, speed = 10, id = 0, name = "skeleton" };
            spawner.units[1] = new Unit { damage = 10, health = 100, speed = 5, id = 1, name = "spider" };
            spawner.units[2] = new Unit { damage = 10, health = 100, speed = 2, id = 2, name = "zombie" };

            //Set up clusters
            spawner.clusters[0] = Cluster.Build("skeleton-spider-gang", new ClusterRule[] { new(0, 3), new(1, 5) });

            //Set up waves
            spawner.waves[0] = Wave.Build("first-wave", new WaveRule[] { new (WaveRuleType.Cluster, 0, -1, 2.0f) });
        }

        /// <summary>
        /// Allocates all native memory and returns a SpawnerDataSingleton which
        /// is set in OnCreate.
        /// </summary>
        /// <returns></returns>
        private SpawnerDataSingleton AllocWaveSpawner()
        {
            //Essentially just used for initialising the native containers
            SpawnerDataSingleton spawner = new SpawnerDataSingleton
            {
                //Note: this assumes there will be a max of 32 types of unit, clusters
                //      and waves. 
                units = new NativeHashMap<int, Unit>(32, Allocator.Persistent),
                clusters = new NativeHashMap<int, Cluster>(32, Allocator.Persistent),
                waves = new NativeHashMap<int, Wave>(32, Allocator.Persistent)
            };

            if(m_Debug)   Debug.Log("Allocated native memory for wave spawner");

            //Somehow there is no SpawnerDataSingleton? If so, add some test data
            //to stop things falling over.
            if(!SystemAPI.HasSingleton<SpawnerDataSingleton>())
                SetupTestSpawner(ref spawner);

            return spawner;
        }

        public void OnDestroy(ref SystemState state)
        {
            //Called when the singleton needs to be deallocated.
            //Add more cleanup logic here as necessary.

            //Get the wave spawner & its entity
            var waveSpawner = SystemAPI.GetSingleton<SpawnerDataSingleton>();
            var waveSpawnerEntity = SystemAPI.GetSingletonEntity<SpawnerDataSingleton>();

            //Call dealloc
            DeallocWaveSpawner(ref waveSpawner);
        }
        public void OnStopRunning(ref SystemState state) { }
    }
}
