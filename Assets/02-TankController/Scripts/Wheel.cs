using UnityEngine;

namespace _02_TankController.Scripts
{
    public class Wheel : MonoBehaviour
    {
        /// <summary>
        /// Applies drive force at this wheel's position.
        /// </summary>
        public void AddDriveForce(Rigidbody rb, float force, Vector3 direction)
        {
            rb.AddForceAtPosition(direction * force, transform.position, ForceMode.Acceleration);
        }

        /// <summary>
        /// Applies braking force (drag) to this wheel.
        /// </summary>
        public void AddBrakingForce(Rigidbody rb, float brakePower)
        {
            //Calculates a force that opposes the current velocity at this wheel - this means braking now works in all directions
            Vector3 velocityAtWheel = rb.GetPointVelocity(transform.position);
            
            // Apply force in the opposite direction of movement
            Vector3 counterForce = -velocityAtWheel.normalized * brakePower;
            
            rb.AddForceAtPosition(counterForce, transform.position, ForceMode.Acceleration);
        }
    }
}
