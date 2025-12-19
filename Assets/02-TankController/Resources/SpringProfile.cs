#region

using UnityEngine;

#endregion

namespace _02_TankController.Resources
{
    [CreateAssetMenu(fileName = "SpringProfile", menuName = "Scriptable Objects/SpringProfile")]
    public class SpringProfile : ScriptableObject
    {
        [SerializeField] [Range(1f, 100f)] public float m_Stiffness = 1f;
        [SerializeField] [Range(0f, 10f)] public float m_Damping = 0.25f;
    }
}