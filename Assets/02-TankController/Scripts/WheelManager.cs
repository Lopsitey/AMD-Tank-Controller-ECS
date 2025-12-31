#region

using System.Collections;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class WheelManager : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float m_TankSpeed = 3.5f;
        [SerializeField] private float m_BrakingForce = 2f;


        private Track[] m_Tracks;

        //Stores all wheels on the left
        private Wheel[] m_LeftWheels;
        private Wheel[] m_RightWheels;

        private Rigidbody m_Rb;
        private Coroutine m_CMove;
        private float m_Acceleration;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();

            m_Tracks = GetComponentsInChildren<Track>();

            if (m_Tracks.Length == 0)
            {
                Debug.LogError("WheelManager could not find any 'Track' scripts in children!");
                enabled = false; //turns off on error
            }

            m_LeftWheels = m_Tracks[0].GetComponentsInChildren<Wheel>();
            m_RightWheels = m_Tracks[1].GetComponentsInChildren<Wheel>();

            if (m_LeftWheels.Length == 0 || m_RightWheels.Length == 0)
            {
                Debug.LogError("WheelManager could not find 'Wheel' scripts in children!");
                enabled = false;
            }
        }

        public void StartAccelerate(float acceleration)
        {
            //If input is negative, clamp speed to -0.75, else use input.
            m_Acceleration = acceleration < 0 ? -0.75f : acceleration; //reversing is 15% slower

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

                for (int i = 0; i < 2; ++i)
                {
                    //swaps the wheel side when i == 1
                    Wheel[] wheels = i == 0 ? m_LeftWheels : m_RightWheels;
                    //gets the traction for the correct side of the vehicle
                    float currentTraction = m_Tracks[i].TractionPercent;
                    //applies force to the individual wheels
                    foreach (var wheel in wheels)
                    {
                        //The wheel uses its own script to apply the force
                        wheel.AddDriveForce(m_Rb, m_Acceleration * m_TankSpeed, currentTraction, forward);
                    }
                }
            }

            m_CMove = null; //so it can be started again

            //below the null assignment so can start moving earlier
            //you're not forced to wait for the tank to come to a full stop
            //only starts braking when there is no input (acceleration == 0)
            while (m_Acceleration == 0 &&
                   m_Rb.linearVelocity.sqrMagnitude > 0.05f) //sqrMagnitude checks the overall speed 
            {
                yield return new WaitForFixedUpdate();

                for (int i = 0; i < 2; ++i)
                {
                    Wheel[] wheels = i == 0 ? m_LeftWheels : m_RightWheels;
                    float currentTraction = m_Tracks[i].TractionPercent;
                    foreach (var wheel in wheels)
                    {
                        wheel.AddBrakingForce(m_Rb, m_BrakingForce, currentTraction);
                    }
                }
            }
        }

        public void EndAccelerate()
        {
            m_Acceleration = 0;
        }
    }
}