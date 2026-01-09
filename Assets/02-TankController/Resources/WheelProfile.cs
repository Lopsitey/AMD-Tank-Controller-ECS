#region

using UnityEngine;

#endregion

namespace _02_TankController.Resources
{
    [CreateAssetMenu(fileName = "WheelProfile", menuName = "Scriptable Objects/WheelProfile")]
    public class WheelProfile : ScriptableObject
    {
        [Tooltip("The factor of force applied to the wheel torque. " +
                 "This should be higher than the actual movement force for clean visuals.")]
        [SerializeField]
        [Range(5f, 10f)]
        public float m_TorqueFactor = 5f;

        [SerializeField] public SpringProfile m_HorizontalSpring;
    }
}