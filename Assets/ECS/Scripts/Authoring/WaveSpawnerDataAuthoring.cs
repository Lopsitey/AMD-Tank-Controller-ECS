#region

using System;
using System.Collections.Generic;
using ECS.wave_spawning;
using Unity.Entities;
using UnityEngine;

#endregion

namespace wave_spawning
{
    #region Units

    /// <summary>
    /// Represents a unit to be spawned. This is a serializable
    /// class which is used in WaveSpawnerDataAuthoring.
    /// </summary>
    [Serializable]
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
    [Serializable]
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
    [Serializable]
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
    [Serializable]
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
    [Serializable]
    public class WaveRuleAuthoring
    {  
        /// <summary>
        /// The ID used to look up either a cluster or unit in the respective HashMap.
        /// This is a key value, not an array index.
        /// </summary>
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
        public UnityObjectRef<TomBenWaveData> waveData;
        public bool debug;
    }

    /// <summary>
    /// Authoring script for wave spawner data. At the moment, 
    /// the values are editable through the inspector. But you can
    /// imagine how straight-forward it would be to set these
    /// values after parsing a TomBen wave spawning file.
    /// </summary>
    public class WaveSpawnerDataAuthoring : MonoBehaviour
    {
        [Header("Wave Spawning Data")] [SerializeField]
        public TomBenWaveData m_TomBenWaveData;
        
        // This is a tag component used to identify the entity which holds the unit prefab buffer
        public struct UnitPrefabRegistryTag : IComponentData { }
        
        // This is a single entry in the prefab list
        public struct UnitPrefabElement : IBufferElementData
        {
            public int UnitID;
            public Entity PrefabEntity;
        }
        
        //Serializable so it can be edited in the list in the inspector
        [Serializable]
        public struct PrefabEntry
        {
            public int unitID;
            public GameObject prefab;
        }
        
        [Header("Unit Prefabs")]
        [SerializeField] public List<PrefabEntry> m_Prefabs;
        
        [Header("Options")] [Tooltip("Whether to print out verbose debug messages")]
        public bool m_Debug;

        /// <summary>
        /// Bakes a WaveSpawnerDataAuthoring to a
        /// SpawnerDataWrapper, which is read later by
        /// a WaveSpawnerSingletonSystem for deferred baking.
        /// </summary>
        private class WaveSpawnerDataBaker : Baker<WaveSpawnerDataAuthoring>
        {
            public override void Bake(WaveSpawnerDataAuthoring authoring)
            {
                if (!authoring.m_TomBenWaveData)
                {
                    Debug.LogError(
                        $"WaveSpawnerDataAuthoring on {authoring.name} is missing the TomBenWaveData ScriptableObject!");
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.None);

                // Passes the ScriptableObject directly to the wrapper
                AddComponent(entity, new SpawnerDataWrapper
                {
                    waveData = authoring.m_TomBenWaveData,
                    debug = authoring.m_Debug
                });

                if (authoring.m_Prefabs.Count == 0)
                {
                    Debug.LogError(
                        $"WaveSpawnerDataAuthoring on {authoring.name} has no prefabs in the prefab registry!");
                    return;
                }

                // Adds the tag to identify the prefab registry entity
                AddComponent(entity, new UnitPrefabRegistryTag());

                // Creates the buffer to hold the prefabs
                DynamicBuffer<UnitPrefabElement> buffer = AddBuffer<UnitPrefabElement>(entity);

                foreach (var entry in authoring.m_Prefabs)
                {
                    buffer.Add(new UnitPrefabElement
                    {
                        UnitID = entry.unitID,
                        PrefabEntity = GetEntity(entry.prefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}