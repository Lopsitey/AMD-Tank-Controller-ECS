#region

using UnityEngine;

#endregion

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
        public void AddDriveForce(Rigidbody tankRb, float force, float traction, Vector3 direction)
        {
            //If there is no traction this will be 0 so the force applied will also be 0
            float totalForce = force * traction;
            tankRb.AddForceAtPosition(direction * totalForce, transform.position, ForceMode.Acceleration);
            m_Rb.AddTorque(transform.right * force, ForceMode.Acceleration); //visual element
        }

        /// <summary>
        /// Applies braking force (drag) to this wheel.
        /// </summary>
        public void AddBrakingForce(Rigidbody tankRb, float brakePower, float traction)
        {
            //Calculates a force that opposes the current velocity at this wheel - this means braking now works in all directions
            Vector3 velocityAtWheel = tankRb.GetPointVelocity(transform.position);

            //Ensures braking only works if there is traction
            float totalBrakePower = brakePower * traction;

            //Apply force in the opposite direction of movement
            Vector3 counterForce = -velocityAtWheel.normalized * totalBrakePower;

            tankRb.AddForceAtPosition(counterForce, transform.position, ForceMode.Acceleration);
            //Ensures the spin is always in the opposite direction
            Vector3 counterTorque = -m_Rb.angularVelocity * brakePower;
            m_Rb.AddTorque(counterTorque, ForceMode.Acceleration);
        }
    }
}