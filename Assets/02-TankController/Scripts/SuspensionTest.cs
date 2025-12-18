using System;
using UnityEngine;

namespace _02_TankController.Scripts
{
    public class SuspensionTest : MonoBehaviour
    {
        [SerializeField] private float m_Stiffness;
        [SerializeField] private Vector3 m_restPos;
        
        private Rigidbody m_rb;
        private Vector3 m_displacement;
        private Vector3 m_force;
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_restPos=transform.position;
        }

        private void FixedUpdate()
        {
            //how far it has moved from its original position
            Debug.DrawLine(transform.position, m_restPos, Color.red);
            //stored in a var
            m_displacement=transform.position-m_restPos;
            
            //stores and applies force the using how far it's moved (displacement)
            m_force = -m_Stiffness * m_displacement;
            m_rb.AddForce(m_force, ForceMode.Acceleration);//acceleration so it's not an instant force
            
            //locks all axis to 0 apart from x (Vector3.right = 1,0,0)
            Vector3 localPos = transform.localPosition;
            localPos.Scale(Vector3.right);
            transform.localPosition = localPos;
        }
    }
}
