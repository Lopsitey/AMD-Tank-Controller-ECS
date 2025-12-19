using System;
using UnityEngine;

namespace _02_TankController.Scripts
{
    public class Wheel : MonoBehaviour
    {
        private Rigidbody m_Rb;
        private void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Applies drive force at this wheel's position.
        /// </summary>
        public void AddDriveForce(Rigidbody tankRb, float force, Vector3 direction)
        {
            tankRb.AddForceAtPosition(direction * force, transform.position, ForceMode.Acceleration);
            //m_Rb.AddTorque(Vector3.right, ForceMode.Acceleration);//visual element
        }

        /// <summary>
        /// Applies braking force (drag) to this wheel.
        /// </summary>
        public void AddBrakingForce(Rigidbody tankRb, float brakePower)
        {
            //Calculates a force that opposes the current velocity at this wheel - this means braking now works in all directions
            Vector3 velocityAtWheel = m_Rb.GetPointVelocity(transform.position);
            
            // Apply force in the opposite direction of movement
            Vector3 counterForce = -velocityAtWheel.normalized * brakePower;
            
            tankRb.AddForceAtPosition(counterForce, transform.position, ForceMode.Acceleration);
            //m_Rb.AddTorque(Vector3.left, ForceMode.Acceleration); make work normally
        }
    }
}
