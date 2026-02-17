using ECS.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        public float m_SpawnDelay;

        private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
        {
            //An entity acts as an ID associated with individual components that contain data about the entity.
            //Unlike GameObjects, entities contain no code: they're units of data that the systems you create process.
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                // Gets the spawner entity itself (primary entity)
                // If this was TransformUsageFlags.None the spawner would be static and wouldn't move
                Entity spawnerEntity = GetEntity(TransformUsageFlags.Dynamic);
                // Creates a fresh spawner component to add to the spawner entity 
                // This just fills out the component data - nothing spawned yet
                AddComponent(spawnerEntity, new EnemySpawnerComponent
                {
                    // Params the data from the author to the baker
                    // Sets the entity prefab to spawn as a default value - this will be set by the wave spawner later
                    entityToSpawn = Entity.Null,
                    spawnDelay = authoring.m_SpawnDelay,// The only value actually passed through
                    spawnPos = Vector3.zero,// More defaults to be set later
                    timer = 0.0f,
                    name = string.Empty,
                    totalToSpawn = 0,
                    spawnedCount = 0
                });
                Debug.Log($"Baking {authoring.name} to {spawnerEntity.Index}");
            }
        }
    }
}
