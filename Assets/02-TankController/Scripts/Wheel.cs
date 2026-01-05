#region

using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class Wheel : MonoBehaviour
    {
        private Rigidbody m_Rb;
        private const float TorqueFactor = 1.33f;
        private float m_BrakePower;
        private Quaternion m_StartRot;

        private void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Applies drive force at this wheel's position.
        /// </summary>
        public void AddDriveForce(Rigidbody tankRb, float force, float traction, Vector3 direction)
        {
            m_BrakePower = 0;
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
            bool braking = m_BrakePower > 0;
            float totalForce = braking ? m_BrakePower * traction : rawForce * traction;

            if (!braking)
            {
                m_Rb.AddRelativeTorque(direction * (totalForce * TorqueFactor), ForceMode.Acceleration);
            }
            else
            {
                //Ensures the spin is always in the opposite direction
                m_Rb.AddRelativeTorque(-m_Rb.angularVelocity * (totalForce * TorqueFactor), ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Applies braking force (drag) to this wheel.
        /// </summary>
        public void AddBrakingForce(Rigidbody tankRb, float brakePower, float traction)
        {
            m_BrakePower = brakePower;
            //Calculates a force that opposes the current velocity at this wheel - this means braking now works in all directions
            Vector3 velocityAtWheel = tankRb.GetPointVelocity(transform.position);

            //Ensures braking only works if there is traction
            float totalBrakePower = m_BrakePower * traction;

            //Apply force in the opposite direction of movement
            Vector3 counterForce = -velocityAtWheel.normalized * totalBrakePower;
            tankRb.AddForceAtPosition(counterForce, transform.position, ForceMode.Acceleration);
        }

        private void FixedUpdate()
        {
            Vector3 currentLocalEuler = transform.localEulerAngles;

            //Locks y and z rotation
            transform.localEulerAngles = new Vector3(currentLocalEuler.x, 0f, 0f);

            // This stops the physics engine from "trying" to rotate it, saving energy/jitter.
            //Converts world ang vel to local 
            Vector3 localAngVel = transform.InverseTransformDirection(m_Rb.angularVelocity);
            //changes the velocity using the local axis
            localAngVel.y = 0;
            localAngVel.z = 0;
            //Applies the local data back to world
            //Can't change the world directly since it wouldn't be relative to the world as opposed to the object
            m_Rb.angularVelocity = transform.TransformDirection(localAngVel);
        }
    }
}