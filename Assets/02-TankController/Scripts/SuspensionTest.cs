#region

using _02_TankController.Resources;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

namespace _02_TankController.Scripts
{
    public class SuspensionTest : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private SpringProfile m_SpringProfile;
        [Header("Start Position")]
        [SerializeField] private Vector3 m_RestPos;
        
        private float m_Stiffness;
        private float m_Damping;
        
        private Quaternion m_StartRot;
        private Rigidbody m_Rb;
        
        private void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            m_RestPos = transform.localPosition;
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
            //from the parent's pos to the child's pos
            Debug.DrawLine(transform.parent.position, transform.position, Color.red);
            
            //Moves along the horizontal axis by default (could be the left or the right axis)
            Vector3 springAxis = -Vector3.right;
            
            //Local coordinates are mainly relevant when your object has a parent
            //when you rotate an object you are changing its coordinates relative to its parent
            //this means if it has no parent, they are essentially its world coordinates
            
            //the direction and distance (magnitude) from its original position
            Vector3 displacement = transform.localPosition - m_RestPos;
            //how much of the displacement is along the downwards vector - how far it has been displaced 
            float displacementDown = Vector3.Dot(displacement, springAxis);

            //This turns the local axis into the relevant world direction
            Vector3 worldAxis = transform.TransformDirection(springAxis);
            
            //how much of the velocity is in the downwards vector
            float velocityDown = Vector3.Dot(m_Rb.linearVelocity, worldAxis);

            //stores the quantity of force to be applied along that vector
            float forceMag = (-m_Stiffness * displacementDown) - (m_Damping * velocityDown); // -k * x - c * v
            //the final part of the equation damps the velocity
            //stores and applies force (in the down direction) using how far it's moved (displacement)
            Vector3 force = springAxis * forceMag;
            //adds force using local coordinates
            m_Rb.AddRelativeForce(force, ForceMode.Acceleration); //acceleration so it's not an instant force

            //creates a location from the displacement axis and the magnitude of displacement along that axis 
            Vector3 displacementDir = springAxis * displacementDown;
            //displacementDir will only ever be on one axis so this locks the object to that axis
            transform.localPosition =
                m_RestPos + displacementDir; //m_rest pos just gives a starting point for that vector
            //essentially just a less hard-coded version of: transform.localPosition = transform.localPosition.Scale(Vector3.right)

            //locks the rotation
            transform.localRotation = m_StartRot;
        }
    }
}