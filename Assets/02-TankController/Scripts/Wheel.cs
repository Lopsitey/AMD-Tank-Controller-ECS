#region

using _02_TankController.Resources;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class Wheel : MonoBehaviour
    {
        [SerializeField]private WheelProfile m_WheelProfile;
        
        private const float TorqueFactor = -5f;
        private SpringProfile m_HorizontalSpring;
        private float m_AlignmentDamping = 6f;//Resistance
        private float m_AlignmentStrength = 75f;
        
        private Rigidbody m_Rb;
        private Quaternion m_StartRot;

        private void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            m_Rb.inertiaTensorRotation = Quaternion.identity;
            m_Rb.inertiaTensor = new Vector3(1f, 10f, 10f);

            if (m_WheelProfile)
            {
                //Negative ensures it spins in reverse to the direction of travel
                TorqueFactor = -WheelProfile.torquefactor;
            }
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
            //The wheel's axel
            Vector3 currentAxle = transform.right;
            //The suspension or tank axel
            Vector3 targetAxle = transform.parent.right;
            
            // The Cross Product finds the vector difference between two directions
            // This vector will point along the axis between the other two
            // This is perfect to use for realigning wheels
            Vector3 alignmentError = Vector3.Cross(currentAxle, targetAxle);
            
            // Gets all rotation
            Vector3 globalAngularVel = m_Rb.angularVelocity;
            
            // Returns the spin along the Axle vector
            Vector3 spinComponent = Vector3.Project(globalAngularVel, currentAxle);
            
            // Anything else is bad wobble along the wrong axis
            Vector3 wobbleComponent = globalAngularVel - spinComponent;
            
            // (Error * Spring) - (Wobble * Damping)
            float springStrength = m_AlignmentStrength;
    
            //Follows hooke's law to make a spring in the direction of the error, F = -kx - cv
            //Since this is a horizontal spring the negative kx is just kx for the force to be applied in the correct direction
            //- CV is just the amount (velocity) of wobble * the damping 
            Vector3 stabilizeTorque = (alignmentError * springStrength) - (wobbleComponent * m_AlignmentDamping);
    
            // Apply in World Space (Vectors are already World Space)
            m_Rb.AddTorque(stabilizeTorque, ForceMode.Acceleration);
        }
    }
}