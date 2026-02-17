using System;
using Unity.Collections;
using Unity.Entities;
using wave_spawning;

// This namespace contains unmanaged data structures for wave spawning data.
// These are set by the WaveSpawnerSingletonSystem and are converted from the managed types found in the
// WaveSpawner namespace (e.g. WaveSpawner.UnitAuthoring).

// Note: These are mostly non-component structs, as they are not
// ECS data. They are simply containers that contain wave spawning data.

// This data is stored in a singleton component (SpawnerDataSingleton)
// it is converted from managed to unmanaged types in the WaveSpawnerSingletonSystem.


namespace ECS.wave_spawning
{
    #region Units
    /// <summary>
    /// ECS wave spawning data regarding an in-game
    /// Unit. Its managed equivalent is WaveSpawner.UnitAuthoring.
    /// </summary>
    public struct Unit
    {
        public int id;
        public FixedString32Bytes name;
        public float damage;
        public float speed;
        public float health;
    }
    #endregion
    #region Clusters
    /// <summary>
    /// ECS wave spawning data regarding an in-game Cluster. Its
    /// managed equivalent is WaveSpawner.ClusterAuthoring.
    /// </summary>
    public struct Cluster
    {
        public NativeArray<ClusterRule> clusterRules;
        public FixedString32Bytes name;

        /// <summary>
        /// Builds a cluster, given a name (for the cluster) and an array
        /// of cluster rules.
        /// </summary>
        /// <param name="name">The name of the cluster</param>
        /// <param name="rules">An array of cluster rules for this cluster.</param>
        /// <returns>A cluster.</returns>
        public static Cluster Build(string name, ClusterRule[] rules)
        {
            //Construct a new cluster, set the name
            Cluster cluster = new Cluster
            {
                name = name,
                //This assumes the array of cluster rules will always be a fixed size.
                //Uses the persistent allocator as this will live for the lifetime of the ECS world.
                //This means it is being managed manually so has to be disposed of manually.
                clusterRules = new NativeArray<ClusterRule>(rules.Length, Allocator.Persistent)
            };

            //Copy rule data
            for (int i = 0; i < rules.Length; i++)
                cluster.clusterRules[i] = rules[i];

            return cluster;
        }


        /// <summary>
        /// Builds a cluster, given a name and array of cluster rules. Note that
        /// this uses ClusterRuleAuthoring rather than cluster rule.
        /// </summary>
        /// <param name="name">The name of the cluster</param>
        /// <param name="rules">An array of cluster rules for this cluster.</param>
        /// <returns>A cluster.</returns>
        public static Cluster BuildFromAuthoring(string name, ClusterRuleAuthoring[] rules)
        {
            //Construct, set name
            Cluster cluster = new Cluster
            {
                name = name,
                //Allocate a native container for the cluster rules. This assumes a fixed size.
                clusterRules = new NativeArray<ClusterRule>(rules.Length, Allocator.Persistent)
            };

            //Copy over cluster rules, convert from managed -> unmanaged
            for (int i = 0; i < rules.Length; i++)
                cluster.clusterRules[i] = new ClusterRule(rules[i].unitId, rules[i].amount);

            return cluster;
        }

        /// <summary>
        /// Called when this cluster is disposed of, deallocates
        /// the native container associated with cluster rules.
        /// </summary>
        public void Dispose()
        {
            clusterRules.Dispose();
        }
    }

    /// <summary>
    /// ECS wave spawning data regarding a cluster rule. Its managed
    /// equivalent is a WaveSpawner.ClusterRuleAuthoring.
    /// </summary>
    public struct ClusterRule
    {
        public int unitId;
        public int amount;

        public ClusterRule(int unitId, int amount)
        {
            this.unitId = unitId;
            this.amount = amount;
        }
    }
    #endregion
    #region Waves
    /// <summary>
    /// ECS wave spawning data regarding an in-game wave. Its
    /// managed equivalent is WaveSpawner.WaveAuthoring.
    /// </summary>
    public struct Wave : IDisposable
    {
        public NativeArray<WaveRule> waveRules;
        public FixedString32Bytes name;

        /// <summary>
        /// Builds a wave given a name and array of wave rules.
        /// </summary>
        /// <param name="name">The name of the wave</param>
        /// <param name="rules">The set of rules</param>
        /// <returns>A constructed wave object</returns>
        public static Wave Build(string name, WaveRule[] rules)
        {
            //Make a new wave with a name
            Wave wave = new Wave();
            wave.name = name;

            //Allocate a native container for wave rules, assumes fixed size
            wave.waveRules = new NativeArray<WaveRule>(rules.Length, Allocator.Persistent);

            //Copy over the rules
            for (int i = 0; i < rules.Length; i++)
                wave.waveRules[i] = rules[i];

            return wave;
        }

        /// <summary>
        /// Builds a wave given a name and array of wave rules, where the
        /// rules are authoring (managed) equivalents.
        /// </summary>
        /// <param name="name">The name of the wave</param>
        /// <param name="rules">The set of rules</param>
        /// <returns>A constructed wave object</returns>
        public static Wave BuildFromAuthoring(string name, WaveRuleAuthoring[] rules)
        {
            //Make a wave with a name
            Wave wave = new Wave();
            wave.name = name;

            //Allocate a native container for the wave rules, assumes fixed size
            wave.waveRules = new NativeArray<WaveRule>(rules.Length, Allocator.Persistent);

            //Copy over wave rules -- construct and copy over to unmanaged type
            for (int i = 0; i < rules.Length; i++)
            {
                wave.waveRules[i] = new WaveRule
                {
                    clusterOrTypeId = rules[i].clusterOrTypeId,
                    triggerPopulationCap = rules[i].triggerPopulationCap,
                    triggerTimeSinceLastSpawn = rules[i].triggerTimeSinceLastSpawn,
                    type = rules[i].type
                };
            }

            return wave;
        }

        /// <summary>
        /// Disposes of the wave rules, which are a native container.
        /// </summary>
        public void Dispose()
        {
            waveRules.Dispose();
        }
    }

    /// <summary>
    /// Represents the type of thing in the wave rule. This
    /// is needed because of the clusterOrTypeId member which
    /// could either refer to a cluster or unit.
    /// </summary>
    public enum WaveRuleType
    {
        Cluster, Unit
    }

    /// <summary>
    /// Represents a wave rule: the type of thing to spawn, the
    /// id of the unity/cluster & the spawn conditions.
    /// </summary>
    public struct WaveRule
    {
        /// <summary>
        /// The ID used to look up either a cluster or unit in the respective HashMap.
        /// When type == WaveRuleType.Cluster, this is used as a key in waveSpawnerData.clusters[clusterOrTypeId].
        /// When type == WaveRuleType.Unit, this is used as a key in waveSpawnerData.units[clusterOrTypeId].
        /// </summary>
        public int clusterOrTypeId;
        public int triggerPopulationCap;
        public float triggerTimeSinceLastSpawn;
        public WaveRuleType type;

        public WaveRule(WaveRuleType type, int clusterOrTypeId, int triggerPopulationCap, float triggerTimeSinceLastSpawn)
        {
            this.type = type;
            this.clusterOrTypeId = clusterOrTypeId;
            this.triggerPopulationCap = triggerPopulationCap;
            this.triggerTimeSinceLastSpawn = triggerTimeSinceLastSpawn;
        }
    }
    #endregion

    /// <summary>
    /// This is a singleton component which is created by
    /// the WaveSpawnerSingletonSystem. This represents the
    /// singleton in the ECS world you can access to get all
    /// the relevant wave spawner data.
    /// 
    /// The data in this singleton is set in OnStartRunning of
    /// the WaveSpawnerSingletonSystem. That system looks for
    /// the existence of a SpawnerDataWrapper (which is baked).
    ///
    /// When it is baked, and OnStartRunning is called, that system
    /// will construct this component and set the relevant data.
    ///
    /// So the flow is:
    /// The managed WaveSpawnerDataAuthoring (with managed data) -->
    /// The WaveSpawnerSingletonSystem (OnStartRunning) -->
    /// SpawnerDataWrapper (baked)--> OnStartRunning -->
    /// This SpawnerDataSingleton component is constructed
    /// which contains the functions to convert the managed data to unmanaged data --> and then set relevant data.
    /// Then, other systems (e.g. WaveSpawnerSystem) can access this singleton to get the data they need.
    /// Lots of these functions are used in the WaveSpawnerSingletonSystem
    /// 
    /// -----------------------
    /// 
    /// Note:   These are hash maps - they are good for fast lookups.
    ///         This is because the wave spawning data uses non-contiguous indices.
    ///         For example, you might parse a file with
    ///         wave id 1, and wave id 5, and no others.
    ///         
    ///         Therefore, these hash maps are look-ups -- they
    ///         convert:
    ///         - unit ids (int key) to a Unit
    ///         - cluster ids (int key) to a Cluster
    ///         - wave ids (int key) to a Wave.
    ///         
    ///         See WaveSpawnerSystem for some example usage on how
    ///         to use these data structures.
    ///
    /// </summary>
    public struct SpawnerDataSingleton : IComponentData
    {
        public NativeHashMap<int, Unit> units;
        public NativeHashMap<int, Cluster> clusters;
        public NativeHashMap<int, Wave> waves;
    }
}

