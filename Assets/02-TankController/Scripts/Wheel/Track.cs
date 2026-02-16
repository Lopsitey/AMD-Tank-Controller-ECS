#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts.Wheel
{
    public class Track : MonoBehaviour
    {
        [Header("Friction Settings")]
        [SerializeField] private float m_SideFriction = 500f;
        [SerializeField] private float m_RollingFriction = 100f;
        
        private List<Suspension> m_SuspensionArms;
        private Rigidbody m_TankRb;
        
        public float TractionPercent { get; private set; }

        private void Awake()
        {
            //Gets all the suspension
            m_SuspensionArms = GetComponentsInChildren<Suspension>().ToList();
            m_TankRb = GetComponentInParent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            int groundedCount = 0;
            //Counts how many arms are currently grounded
            foreach (var arm in m_SuspensionArms)
            {
                if (arm.IsGrounded)
                {
                    ++groundedCount;
                    //Only allow friction if the wheel is grounded
                    //More wheels grounded = more friction
                    ApplyFriction(arm);
                }
            }

            //The overall traction - not calculated if no wheels are on the ground
            TractionPercent = 0;
            if (m_SuspensionArms.Count > 0)
                TractionPercent = Mathf.Clamp01((float)groundedCount / m_SuspensionArms.Count);
        }
        
        /// <summary>
        /// Applies friction to the individual wheels using the traction percentage
        /// </summary>
        /// <param name="arm">The suspension arm for the wheel</param>
        private void ApplyFriction(Suspension arm)
        {
            //Arm transform will be aligned with the wheel
            Transform trans = arm.transform;
            Vector3 wheelVel = m_TankRb.GetPointVelocity(trans.position);

            //Sideways speed amount
            float slideSpeed = Vector3.Dot(wheelVel, trans.right);
            //Applies friction in the opposing direction
            Vector3 sideForce = -trans.right * (slideSpeed * m_SideFriction);
            
            //Forwards speed amount
            float rollSpeed = Vector3.Dot(wheelVel, trans.forward);
            Vector3 dragForce = -trans.forward * (rollSpeed * m_RollingFriction);
            
            Vector3 totalFriction = sideForce + dragForce;
            m_TankRb.AddForceAtPosition(totalFriction, trans.position, ForceMode.Acceleration);
        }
    }
}