#region

using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class SuspensionTest : MonoBehaviour
    {
        [SerializeField] private float m_Stiffness;
        [SerializeField] private Vector3 m_restPos;
        
        private Quaternion m_startRot;
        private Rigidbody m_rb;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_restPos = transform.localPosition;
            m_startRot = transform.rotation;
        }

        private void FixedUpdate()
        {
            //Hooke's law: F = -kx
            //Where F is the force being calculated and K is the stiffness and X is the displacement
            
            //how far it has moved from its original position
            //Debug.DrawLine(transform.position, m_restPos, Color.red);
            Debug.DrawLine(transform.parent.position, transform.position, Color.red);//from the parent's pos to the child's pos
            //stored in a var
            Vector3 displacement = transform.localPosition - m_restPos;
            Vector3 localDown = transform.worldToLocalMatrix.MultiplyVector(-transform.right); 
            
            //how much of the displacement is along the downwards vector 
            float displacementDown = Vector3.Dot(displacement, localDown);
            //the quantity of force
            float forceMag = -m_Stiffness * displacementDown;// -k * x
            
            //stores and applies force (in the down direction) using how far it's moved (displacement)
            Vector3 force = localDown * forceMag;
            m_rb.AddForce(force, ForceMode.Acceleration); //acceleration so it's not an instant force

            //locks all axis to 0 apart from x (Vector3.right = 1,0,0)
            Vector3 localPos = transform.localPosition;
            localPos.Scale(Vector3.right);
            transform.localPosition = localPos;
            //locks the rotation
            transform.localRotation = m_startRot;
        }
    }
}