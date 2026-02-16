#region

using UnityEngine;

#endregion

namespace _02_TankController.Resources
{
    [CreateAssetMenu(fileName = "ArmProfile", menuName = "Scriptable Objects/ArmProfile")]
    public class ArmProfile : ScriptableObject
    {
        [Header("Profile & Mask")] [SerializeField]
        public SpringProfile m_SpringProfile;

        [SerializeField] public LayerMask m_GroundLayerMask;

        [Header("Spring and Wheel")] [SerializeField] 
        [Range(0.25f,1f)] public float m_SpringLength = 0.2f;

        [SerializeField] [Range(0, 0.5f)] public float m_WheelRadius = 0.3f;
    }
}