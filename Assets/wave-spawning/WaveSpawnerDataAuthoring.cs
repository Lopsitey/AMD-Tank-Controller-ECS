using Unity.Entities;
using UnityEngine;

namespace ECS.wave_spawning
{
    #region Units
    /// <summary>
    /// Represents a unit to be spawned. This is a serializable
    /// class which is used in WaveSpawnerDataAuthoring.
    /// </summary>
    [System.Serializable]
    public class UnitAuthoring
    {
        public int id;
        public string name;
        public float damage;
        public float speed;
        public float health;
    }
    #endregion
    #region Clusters
    /// <summary>
    /// Represents a cluster to be spawned.
    /// </summary>
    [System.Serializable]
    public class ClusterAuthoring
    {
        public int id;
        public string name;
        public ClusterRuleAuthoring[] clusterRules;
    }

    /// <summary>
    /// Represents a cluster rule for a cluster, specifically
    /// the unit to spawn, and the amount of to spawn of that unit.
    /// </summary>
    [System.Serializable]
    public class ClusterRuleAuthoring
    {
        public int unitId;
        public int amount;
    }
    #endregion
    #region Waves
    /// <summary>
    /// Represents a wave of clusters/units to be spawned. Essentially a wrapper around many wave rules.
    /// </summary>
    [System.Serializable]
    public class WaveAuthoring
    {
        public int id;
        public string name;
        public WaveRuleAuthoring[] waveRules;
    }

    /// <summary>
    /// A single wave rule -- the cluster or type to spawn,
    /// the conditions in which the wave rule should trigger spawn,
    /// and the type of thing to spawn.
    /// </summary>
    [System.Serializable]
    public class WaveRuleAuthoring
    {
        public int clusterOrTypeId;
        public int triggerPopulationCap;
        public float triggerTimeSinceLastSpawn;
        public WaveRuleType type;
    }
    #endregion

    /// <summary>
    /// This is a wrapper component which holds a reference to
    /// a WaveSpawnerDataAuthoring. This is baked into the ECS world.
    /// 
    /// The WaveSpawnerSingletonSystem looks for this type of component,
    /// and if found, allocates native memory, copying across the values
    /// in this component.
    /// 
    /// This is done because it is difficult to allocate native memory
    /// at bake time. Instead, we must defer the allocation until the 
    /// system starts running -- whereupon we can allocate memory.
    /// </summary>
    public struct SpawnerDataWrapper : IComponentData
    {
        public UnityObjectRef<WaveSpawnerDataAuthoring> spawnerAuthoring;
        public bool debug;
    }

    /// <summary>
    /// Authoring script for wave spawner data. At the moment, 
    /// the values are editable through the inspector. But you can
    /// imagine how straight-forward it would be to set these
    /// values after parsing a TomBen wave spawning file.
    /// 
    /// TODO: Make it so these arrays are populated after
    /// parsing the format, rather than at edit time.
    /// </summary>
    public class WaveSpawnerDataAuthoring : MonoBehaviour
    {
        [Header("Wave spawning data")]
        public UnitAuthoring[] m_units;
        public ClusterAuthoring[] m_clusters;
        public WaveAuthoring[] m_waves;

        [Header("Options")]
        [Tooltip("Whether to print out verbose debug messages")]
        public bool m_debug;

        /// <summary>
        /// Bakes a WaveSpawnerDataAuthoring to a
        /// SpawnerDataWrapper, which is read later by
        /// a WaveSpawnerSingletonSystem for deferred baking.
        /// </summary>
        private class WaveSpawnerDataBaker : Baker<WaveSpawnerDataAuthoring>
        {
            public override void Bake(WaveSpawnerDataAuthoring authoring)
            {
                AddComponent(GetEntity(TransformUsageFlags.None), new SpawnerDataWrapper
                {
                    spawnerAuthoring = authoring,
                    debug = authoring.m_debug
                });
            }
        }
    }
}
