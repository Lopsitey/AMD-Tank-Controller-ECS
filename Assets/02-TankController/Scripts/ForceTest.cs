using UnityEngine;

namespace _02_TankController.Scripts
{
    public class ForceTest : MonoBehaviour
    {
        private Rigidbody m_Rb;
        [SerializeField] private float m_YeetPower = 10f; 
        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();

            YeetFunc();
            //ForceFunc();
        }

        private void ForceFunc()
        {
            Vector3 force = new Vector3(10, 10, 0);//10 x and 10 y moves up and right
            Vector3 position = transform.position;
            Vector3 torque = Vector3.forward;
            
            //applies a force to the rigidbody at its centre of mass
            m_Rb.AddForce(force, ForceMode.Impulse);
            //impulse = immediate | acceleration = gradual | force = consistent, like a jetpack
            
            //applies the force at a specified location, like flicking the corner of a card
            m_Rb.AddForceAtPosition(force, position - new Vector3(0, -1, 0), ForceMode.Acceleration);
            //-1 on the y-axis means the force will be applied below the centre of the object

            //uses the objects local co-ordinates to apply force so Vector3.right and forward, etc will always be relative to the way the object is facing 
            m_Rb.AddRelativeForce(Vector3.forward, ForceMode.Acceleration);//good for cars/planes etc

            m_Rb.AddTorque(torque, ForceMode.Force);//good for wheels/propellers - rotational force
        }

        private void YeetFunc()
        {
            //always moves the object forwards relatively, but up using the world-space, if it was moved up relatively then the object would fly into the floor if it was upside-down
            Vector3 yeetDirection = (m_Rb.transform.forward + Vector3.up).normalized;
            
            m_Rb.AddForce(yeetDirection * m_YeetPower, ForceMode.Impulse);
            
            //random torque for making it look realistic
            m_Rb.AddTorque(Random.insideUnitSphere * m_YeetPower, ForceMode.Impulse);
        }
    }
}
