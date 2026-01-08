#region

using UnityEngine;

#endregion

namespace _02_TankController.Resources
{
    [CreateAssetMenu(fileName = "WheelProfile", menuName = "Scriptable Objects/WheelProfile")]
    public class WheelProfile : ScriptableObject
    {
        [Tooltip("The force applied to the wheel torque should be higher than the actual movement force for clean visuals.")]
        [SerializeField] [Range(5f,10f)] public float m_TorqueFactor = 5f;
        private SpringProfile m_HorizontalSpring;
        private float m_AlignmentDamping = 6f;
        private float m_AlignmentStrength = 75f;
    }
}