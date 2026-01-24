using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        public GameObject m_Prefab;
        public float m_SpawnDelay;

        private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                // Gets the spawner entity itself (primary entity)
                // TransformUsageFlags.None means the spawner is static and won't move
                Entity spawnerEntity = GetEntity(TransformUsageFlags.None);
                
                // Creates a fresh spawner component to add to the spawner entity 
                // This just fills out the component data - nothing spawned yet
                AddComponent(spawnerEntity, new EnemySpawnerComponent
                {
                    // Params the data from the author to the baker
                    // Gets the entity associated with the author's prefab field
                    entityToSpawn = GetEntity(authoring.m_Prefab, TransformUsageFlags.Dynamic),
                    spawnDelay = authoring.m_SpawnDelay,
                    spawnPos = authoring.transform.position,
                    timer = 0.0f
                });
                Debug.Log($"Baking {authoring.name} to {spawnerEntity.Index}");
            }
        }
    }
}
