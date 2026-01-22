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
                Entity entity = GetEntity(TransformUsageFlags.None);
                Debug.Log($"Baking{authoring.name} to {entity.Index}");
            }
        }
    }
}
