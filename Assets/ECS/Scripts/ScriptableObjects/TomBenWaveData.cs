using UnityEngine;
using ECS.wave_spawning;

namespace wave_spawning
{
    [CreateAssetMenu(fileName = "TomBenWaveData", menuName = "Scriptable Objects/TomBenWaveData", order = 0)]
    public class TomBenWaveData : ScriptableObject
    {
        [SerializeField] public UnitAuthoring[] Units;
        [SerializeField] public ClusterAuthoring[] Clusters;
        [SerializeField] public WaveAuthoring[] Waves;
    }
}