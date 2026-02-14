using ECS.Scripts.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Scripts.Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        public GameObject m_Prefab;
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
                    // Gets the entity associated with the author's prefab field
                    entityToSpawn = GetEntity(authoring.m_Prefab, TransformUsageFlags.Dynamic),
                    spawnDelay = authoring.m_SpawnDelay,
                    spawnPos = authoring.transform.position,
                    timer = 0.0f,
                    name = authoring.name,
                    spawnCount = 0
                });
                Debug.Log($"Baking {authoring.name} to {spawnerEntity.Index}");
            }
        }
    }
}
