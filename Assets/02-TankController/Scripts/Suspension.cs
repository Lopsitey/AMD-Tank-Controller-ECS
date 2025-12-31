#region

using _02_TankController.Resources;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class Suspension : MonoBehaviour
    {
        [Header("Profile & Mask")] [SerializeField]
        private SpringProfile m_SpringProfile;

        [SerializeField] private LayerMask m_GroundLayerMask;

        [Header("Spring and Wheel")] [SerializeField]
        float m_SpringLength = 0.25f;

        [SerializeField] [Range(0, 0.5f)] private float m_WheelRadius = 0.3f;

        private Rigidbody m_Rb;
        private Transform m_Wheel;

        private Vector3 m_StartPos;
        private Quaternion m_StartRot;

        private float m_Stiffness;
        private float m_Damping;
        private float m_WheelOffset;
        private float m_RaycastHitDist;

        public bool IsGrounded { get; private set; }

        private enum State
        {
            Grounded,
            Airborne
        }

        private State m_GroundState;

        private void Awake()
        {
            m_Rb = GetComponentInParent<Rigidbody>();
            m_Wheel = transform.GetChild(0);

            m_StartPos = m_Wheel.localPosition;
            m_StartRot = m_Wheel.localRotation;
            if (m_SpringProfile)
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

            //Transform.up or.down etc is always your transform in relation to the centre of the world's axis
            //So if you are upside down then you are facing away from the centre of the world and so transform.up will return vector3.down

            //If used in a world context like transform.rotation = Quaternion.LookRotation(Vector3.down),
            //Then pointing up left or right, whatever, vector3.up will always be physically above you, even when facing the floor
            //However, if used in a local context like transform,localRotation = Quaternion.LookRotation(Vector3.down)
            //Then Vector3.down will be down in relation to the parent object if assigned to local coordinates
            //This is why transform. is classed as world coords and vector3. can be used for local or world coords

            //transform.localPosition/localRotation are your relative to the object's parent so if the object has no parent then it is relative to the world
            //these would be good for getting a turret to rotate on a tank
            //this is because rotating a child object and then checking its transform.up will return the transform of the parent

            //Uncompressed by default
            float currentLen = m_SpringLength;

            //a local position would start the ray in completely the wrong place since the function requires world coordinates
            //the ray uses world rotation so the suspension works properly when the tank is upside down
            Vector3 offsetPos = transform.position - new Vector3(0, m_WheelRadius, 0);
            if (Physics.Raycast(offsetPos, -transform.up, out RaycastHit hit, m_SpringLength, m_GroundLayerMask))
            {
                //Hit something? This wheel is grounded
                m_RaycastHitDist = hit.distance; //this var is for the debug
                //When grounded, the length should be compressed by the hit distance towards the spring origin
                currentLen = hit.distance;
                //ensures the event is only fired once, upon state change
                if (m_GroundState == State.Airborne)
                {
                    IsGrounded = true;
                    m_GroundState = State.Grounded;
                }
            }
            else
            {
                //Not hit something? This wheel is not grounded
                m_RaycastHitDist = 0;
                if (m_GroundState == State.Grounded)
                {
                    IsGrounded = false;
                    m_GroundState = State.Airborne;
                }
            }

            //How compressed (out of 1) the spring would be.
            //Calculated using the current length / (total length - wheel size)
            float percentOfSpringLen = Mathf.Clamp01(currentLen / (m_SpringLength - m_WheelRadius));

            //Compression percent is just (1 - that)
            float compressionPercent = 1 - percentOfSpringLen;

            //how far it has been displaced
            float displacement = compressionPercent * m_SpringLength;

            //needs to use world coordinates because the wheel spins so the local down won't always be the same
            //how much of the wheel's velocity is in the downwards vector
            float velocityDown = Vector3.Dot(-transform.up, m_Rb.GetPointVelocity(m_Wheel.transform.position));

            //stores the quantity of force to be applied along that vector
            float forceMag = (-m_Stiffness * displacement) - (m_Damping * velocityDown); // -k * x - c * v
            //the final part of the equation damps the velocity
            //stores and applies force (in the down direction) using how far it's moved (displacement)
            Vector3 force = -transform.up * forceMag; //uses world coords here for the same reason as above
            //Acceleration so it's not an instant force
            m_Rb.AddForceAtPosition(force, m_Wheel.position, ForceMode.Acceleration);

            //creates a location from the displacement axis and the magnitude of displacement along that axis 
            Vector3 displacementDir = Vector3.down * currentLen;
            //displacementDir will only ever be on one axis so this locks the object to that axis
            m_Wheel.localPosition = m_StartPos + displacementDir;
            //where m_StartPos is the origin and the displacement is the dir and magnitude
            //essentially just a less hard-coded version of: transform.localPosition = transform.localPosition.Scale(Vector3.right)
            //locks the rotation
            transform.localRotation = m_StartRot;
            //todo tank wheels can rotate weirdly atm, not sure if that is bcs tank rotation is not fully implemented yet
        }

        private void OnDrawGizmosSelected()
        {
            //Stops if not in the editor
            if (!Application.isPlaying)
                return;

            Vector3 pos = transform.position;

            //Draws the base detection line
            //This is purely the current suspension compression
            Gizmos.color = Color.blueViolet;
            Vector3 endPoint = pos + -transform.up * m_RaycastHitDist;
            pos -= new Vector3(0, m_WheelOffset, 0);
            Gizmos.DrawLine(pos, endPoint);

            //Draws the remainder of the line
            //This is the potential suspension decompression 
            Gizmos.color = Color.blue;
            pos = endPoint;
            endPoint = pos + -transform.up * (m_SpringLength - m_RaycastHitDist);
            Gizmos.DrawLine(pos, endPoint);
        }
    }
}