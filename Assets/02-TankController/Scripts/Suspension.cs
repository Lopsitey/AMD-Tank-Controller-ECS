using _02_TankController.Resources;
using UnityEngine;

namespace _02_TankController.Scripts
{
    public class Suspension : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private SpringProfile m_SpringProfile;
        [Header("Spring and Wheel")]
        [SerializeField] float m_SpringLength = 0.25f;
        [SerializeField] float m_WheelRadius = 1f;
        
        private Rigidbody m_Rb;
        private Transform m_Wheel;
        
        private Vector3 m_StartPos;
        private Quaternion m_StartRot;
        
        private float m_Stiffness;
        private float m_Damping;
        private bool m_HitGround;
        
        private void Awake()
        {
            m_Rb = GetComponentInParent<Rigidbody>();
            m_Wheel = transform.GetChild(0);
            
            m_StartPos = transform.localPosition;
            m_StartRot = transform.localRotation;
            if(m_SpringProfile)
            {
                m_Stiffness = m_SpringProfile.m_Stiffness;
                m_Damping = m_SpringProfile.m_Damping;
            }
            else
            {
                Debug.LogWarning("SuspensionTest: No SpringProfile found!");
            }
        }
    
        private void FixedUpdate()
        { 
            //A vector is just a location created from a direction (the values when normalised)
            //and how far you are in that direction (magnitude - the values when not normalised)

            //Hooke's law: F = -kx
            //Where F is the force being calculated and K is the stiffness and X is the displacement

            //To include damping it is: F = -kx - cv 
            //Where c is a damping constant and v is the velocity of the spring 

            Vector3 springAxis = -transform.up;

            //starting point of the ray needs to be in world coordinates
            Vector3 pos = transform.position;
            m_HitGround = Physics.Raycast(pos, springAxis, out var hitInfo, m_SpringLength);
            
            //Uncompressed by default
            //On hit, the length should be compressed by the hit distance towards the spring origin
            float currentLen = (!m_HitGround) ? (m_SpringLength) : (hitInfo.distance);
            
            //How compressed (out of 1) the spring would be.
            //Calculated using the current length / (total length - wheel size)  
            float percentOfSpringLen = Mathf.Clamp01(currentLen / (m_SpringLength - m_WheelRadius));

            //Compression percent is just (1 - that)
            float compressionPercent = 1 - percentOfSpringLen;
            
            //how far it has been displaced
            float displacement = compressionPercent * m_SpringLength;
            
            //how much of the wheel's velocity is in the downwards vector
            float velocityDown = Vector3.Dot(-m_Wheel.up, m_Rb.GetPointVelocity(m_Wheel.transform.position));

            //stores the quantity of force to be applied along that vector
            float forceMag = (-m_Stiffness * displacement) - (m_Damping * velocityDown); // -k * x - c * v
            //the final part of the equation damps the velocity
            //stores and applies force (in the down direction) using how far it's moved (displacement)
            Vector3 force = -m_Wheel.up * forceMag;
            //Acceleration so it's not an instant force
            m_Rb.AddForceAtPosition(force, m_Wheel.position, ForceMode.Acceleration);
            
            //creates a location from the displacement axis and the magnitude of displacement along that axis 
            Vector3 displacementDir = springAxis * currentLen;
            //displacementDir will only ever be on one axis so this locks the object to that axis
            //m_Wheel.localPosition = m_StartPos + displacementDir; //m_rest pos just gives a starting point for that vector//todo does the wheel rotation need to be fixed to one axis or something?
            //essentially just a less hard-coded version of: transform.localPosition = transform.localPosition.Scale(Vector3.right)
        }
        
        private void OnDrawGizmosSelected()
        {
            //turns red when the suspension is in use - grey when not
            Gizmos.color = m_HitGround ? Color.red : Color.grey;
            if (m_Wheel)
                Gizmos.DrawLine(transform.position, m_Wheel.position);
        }
    }
}
