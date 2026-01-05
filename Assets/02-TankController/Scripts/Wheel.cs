#region

using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class Wheel : MonoBehaviour
    {
        private Rigidbody m_Rb;
        private const float TorqueFactor = 1.33f;
        private Quaternion m_StartRot;
        
        private float m_AlignmentDamping = 3f; // The Damper (Resistance)
        private float m_AlignmentSpeed = 1f; // Strength of the "Magnet" todo - move to manager?

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
        }

        /// <summary>
        /// Applies a visual spin to the wheels
        /// </summary>
        /// <param name="rawForce">Raw input, not affected by throttle</param>
        /// <param name="traction">If the wheels are grounded</param>
        /// <param name="direction">The direction for the wheels to spin</param>
        public void AddTorqueForce(float rawForce, float traction, Vector3 direction)
        {
            float totalForce = rawForce * traction;
            m_Rb.AddRelativeTorque(direction * (totalForce * TorqueFactor), ForceMode.Acceleration);
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
            m_Rb.AddRelativeTorque(-m_Rb.angularVelocity * (totalBrakePower * TorqueFactor), ForceMode.Acceleration);
        }

        private void FixedUpdate()
        {
            // 1. Get Current Alignment (Angles)
            Vector3 localEuler = transform.localEulerAngles;
            
            // Fix the 0-360 wrapping so we get clear + / - errors (e.g., -5 degrees instead of 355)
            float yAngle = localEuler.y > 180 ? localEuler.y - 360 : localEuler.y;
            float zAngle = localEuler.z > 180 ? localEuler.z - 360 : localEuler.z;

            //Applies the local data back to world
            //Can't change the world directly since it wouldn't be relative to the world as opposed to the object
            Vector3 localVel = transform.InverseTransformDirection(m_Rb.angularVelocity);

            // 3. Calculate Correction Torque (PID: Spring - Damper)
            // Spring: "Go to 0!" (-Angle * Speed)
            // Damper: "Don't overshoot!" (-Velocity * Damping)
    
            // Y-Axis (Steering Alignment)
            float yTorque = (-yAngle * m_AlignmentSpeed) - (localVel.y * m_AlignmentDamping);
    
            // Z-Axis (Camber/Roll Alignment)
            float zTorque = (-zAngle * m_AlignmentSpeed) - (localVel.z * m_AlignmentDamping);

            // 4. Apply (Leave X alone! That is your wheel spin)
            Vector3 stabilizeTorque = new Vector3(0f, yTorque, zTorque);
    
            m_Rb.AddRelativeTorque(stabilizeTorque, ForceMode.Acceleration);
        }
    }
}