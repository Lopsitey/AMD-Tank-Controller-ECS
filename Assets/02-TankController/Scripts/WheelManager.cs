using System.Collections;
using UnityEngine;

namespace _02_TankController.Scripts
{
    public class WheelManager : MonoBehaviour
    {
        [Header("Movement")] 
        [SerializeField] private float m_TankSpeed = 3.5f;
        [SerializeField] private float m_BrakingForce = 2f;
        
        //Stores every wheel
        private WheelController[] m_TankWheels; 
        
        private Rigidbody m_Rb; 
        private Coroutine m_CMove;
        private float m_Acceleration;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();

            m_TankWheels = GetComponentsInChildren<WheelController>();

            if (m_TankWheels.Length == 0)
            {
                Debug.LogError("WheelManager could not find any 'WheelController' scripts in children!");
                enabled = false;//turns off on error
            }
        }

        public void StartAccelerate(float acceleration)
        {
            //If input is negative, clamp speed to -0.75, else use input.
            m_Acceleration = acceleration < 0 ? -0.75f : acceleration;//reversing is 15% slower

            if (m_Acceleration != 0f)
            {
                //if no coroutine was running, initialise it
                //ensures only one instance can be started
                m_CMove ??= StartCoroutine(C_MoveUpdate());
            }
        }
        
        IEnumerator C_MoveUpdate()
        {
            Vector3 forward = transform.forward;
           
            while (m_Acceleration != 0)
            {
                yield return new WaitForFixedUpdate();
                
                //applies force to the individual wheels
                foreach (WheelController wheel in m_TankWheels)
                {
                    //The wheel uses its own script to apply the force
                    wheel.AddDriveForce(m_Rb, m_Acceleration * m_TankSpeed, forward);
                }
            }
            
            m_CMove = null;//so it can be started again
            
            //below the null assignment so can start moving earlier
            //you're not forced to wait for the tank to come to a full stop
            //only starts braking when there is no input (acceleration == 0)
            while (m_Acceleration == 0 && m_Rb.linearVelocity.sqrMagnitude > 0.05f)//sqrMagnitude checks the overall speed
            {
                yield return new WaitForFixedUpdate();
                
                foreach (WheelController wheel in m_TankWheels)
                {
                    wheel.AddBrakingForce(m_Rb, m_BrakingForce);
                }
            }
        }
        
        public void EndAccelerate()
        {
            m_Acceleration = 0;
        }
    }
}