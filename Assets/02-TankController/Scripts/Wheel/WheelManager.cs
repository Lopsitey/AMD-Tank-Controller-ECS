#region

using System.Collections;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts.Wheel
{
    public class WheelManager : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float m_TankSpeed = 0.75f;
        [SerializeField] private float m_MaxSpeed = 15f;

        [SerializeField] private float m_BrakingForce = 1.5f;

        [Tooltip("How fast long it should take for the max acceleration to be applied, when starting to move.")]
        [SerializeField]
        private float m_RevSpeed = 0.5f;

        [Tooltip("The fastest the turning track can spin when turning. " +
                 "E.g., when turning right overdrive would be applied to the left track.")]
        [SerializeField]
        [Range(1.5f, 2f)]
        private float m_OverdriveLimit = 1.5f;

        private Track[] m_Tracks;

        //Stores all wheels on the left
        private Wheel[] m_LeftWheels;
        private Wheel[] m_RightWheels;

        private Rigidbody m_Rb;
        private Coroutine m_CMove;

        private float m_ForwardInput;
        private float m_TurnInput;

        //current amount of acceleration being applied
        private float m_CurrentRevs;


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

        public void StartAccelerate(float input)
        {
            //If input is negative, clamp speed to -0.75, else use input.
            m_ForwardInput = input < 0 ? -0.75f : input; //reversing is 25% slower

            if (m_ForwardInput != 0f || m_TurnInput != 0f)
            {
                //if no coroutine was running, initialise it
                //ensures only one instance can be started
                m_CMove ??= StartCoroutine(C_MoveUpdate());
            }
        }

        IEnumerator C_MoveUpdate()
        {
            //if there is input - || ensures the loop doesn't end whilst there is throttle remaining
            while (m_ForwardInput != 0 || (Mathf.Abs(m_CurrentRevs) > 0.01f || Mathf.Abs(m_TurnInput) > 0.01f))
            {
                yield return new WaitForFixedUpdate();
                
                //the amount of velocity in the forward direction
                float forwardSpeed = Vector3.Dot(m_Rb.linearVelocity, transform.forward);
                
                //Speed relative to the max speed
                //Can't just check throttle as it increases quickly and can be released on a hill 
                float speedPercent = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / m_MaxSpeed); 
                
                //At a lower speed the lerp increments by less so it will be closer to 1
                //At a higher speed the lerp will increment more so it will most likely be 0.5
                float turnAmount = Mathf.Lerp(1f, 0.5f, speedPercent);
                //Turns fast when moving slow - realistic, like you would in a car
                //Slower turning when moving fast prevents the tank from spinning out of control

                //Used to limit the actual input so you can't go crazy and spin out
                float restrictedTurnInput = Mathf.Clamp(m_TurnInput, -turnAmount, turnAmount);
                
                //Flips the turn input when reversing 
                float effectiveTurn = m_ForwardInput < 0 ? -restrictedTurnInput : restrictedTurnInput;
                
                //Smoothly moves from the current throttle to the current input, incrementing by the rev speed
                //Can increment by less than the rev speed if the numbers don't line up perfectly on the final incrementation
                m_CurrentRevs = Mathf.MoveTowards(m_CurrentRevs, m_ForwardInput, m_RevSpeed * Time.fixedDeltaTime);
                
                //Clamped because holding w and a would make you move twice as fast
                //The throttle applies the direction to both tracks and is needed to make the left +1 and the right -1 or vice versa
                float leftInput = Mathf.Clamp(m_CurrentRevs + effectiveTurn, -m_OverdriveLimit, m_OverdriveLimit);
                float rightInput = Mathf.Clamp(m_CurrentRevs - effectiveTurn, -m_OverdriveLimit, m_OverdriveLimit);

                //The direction of the track's movement - left going backwards and right going forwards means left and vice versa     
                Vector3 leftDir = leftInput >= 0 ? transform.forward : -transform.forward;
                //If the track has power 
                float leftPower = Mathf.Abs(leftInput);
                
                Vector3 rightDir = rightInput >= 0 ? transform.forward : -transform.forward;
                float rightPower = Mathf.Abs(rightInput);

                //reverse the track opposite to the turn direction 
                if (leftInput >= 0 && rightInput <= 0)
                    rightDir = -transform.forward;
                if (rightInput >= 0 && leftInput <= 0)
                    leftDir = -transform.forward;

                for (int i = 0; i < 2; ++i)
                {
                    //swaps the wheel side when i == 1
                    Wheel[] wheels;
                    Vector3 moveAxis;
                    Vector3 spinAxis = Vector3.zero;
                    float wheelPower;
                    if (i == 0)
                    {
                        wheels = m_LeftWheels;
                        moveAxis = leftDir;
                        wheelPower = leftPower;
                        //if turning left reverse the spin direction
                        if(wheelPower < rightPower)
                            spinAxis = Vector3.left;
                    }
                    else
                    {
                        wheels = m_RightWheels;
                        moveAxis = rightDir;
                        wheelPower = rightPower;
                        if(wheelPower < leftPower)
                            spinAxis = Vector3.left;
                    }

                    //If not turning then
                    //Set the spin direction of the wheels using the tank's movement direction
                    if (spinAxis == Vector3.zero)
                        spinAxis = moveAxis == transform.forward ? Vector3.right : Vector3.left;
                    
                    //gets the traction for the correct side of the vehicle
                    float currentTraction = m_Tracks[i].TractionPercent;
                    //applies force to the individual wheels
                    foreach (var wheel in wheels)
                    {
                        float driveForce = m_ForwardInput == 0 ? 0 : wheelPower * m_TankSpeed;
                        //math.abs limits the reversing velocity as well
                        bool belowLimit = Mathf.Abs(forwardSpeed) < m_MaxSpeed;
                        //only adds force when below the max speed or if turning 
                        if (belowLimit || m_TurnInput != 0)
                        {
                            //If there is force to add
                            if (driveForce != 0)
                            {
                                //The wheel uses its own script to apply the force
                                wheel.AddDriveForce(m_Rb, driveForce, currentTraction, moveAxis);
                            }
                        }
                        wheel.AddTorqueForce(driveForce, currentTraction, spinAxis);
                    }
                }
            }

            m_CMove = null; //so it can be started again
            m_CurrentRevs = 0f; //Clears any unused revs for next cycle (the 0.01f)

            //below the null assignment so can start moving earlier
            //you're not forced to wait for the tank to come to a full stop
            //only starts braking when there is no input (acceleration == 0)
            while (m_ForwardInput == 0 &&
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
            m_ForwardInput = 0;
        }

        public void StartTurn(float input)
        {
            m_TurnInput = input;
        }

        public void EndTurn()
        {
            m_TurnInput = 0;
        }
    }
}