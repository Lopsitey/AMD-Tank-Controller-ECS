#region

using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

#endregion

namespace _02_TankController.Scripts
{
    public class CameraController : MonoBehaviour
    {
        [Header("Cinemachine")] [SerializeField]
        private CinemachineCamera m_VirtualCamera;

        [Header("Follow")] [SerializeField] private Transform m_TankToFollow;
        //how long it takes for the camera to catch the tank
        [SerializeField] private float m_FollowSpeed = 0.1f;
        [SerializeField] private Vector3 m_FollowOffset = new Vector3(0, 1.5f, 0);
        //the max distance before the camera just snaps to the tank
        [SerializeField] private float m_MaxFollowDistance = 10f;

        [Header("Zoom")] [SerializeField] private float m_MinDistance = 5f;//Closest zoom
        [SerializeField] private float m_MaxDistance = 10f;//Furthest zoom
        [SerializeField] private float m_ZoomSpeed = 1f;
        [SerializeField] private float m_ZoomSmoothTime = 0.38f;
        
        [Header("Aim / Orbit")] 
        [SerializeField] private float m_LookSensitivity = 0.5f;
        [SerializeField] private float m_MinXAngleDeg = 10;//could use -10 for looking up
        [SerializeField] private float m_MaxXAngleDeg = 60;//for looking down
        
        [Header("Auto-Aim")]
        [SerializeField] private float m_AutoAimStartDelay = 1f;//How long before auto-aim kicks in
        [SerializeField] private float m_AutoAimSpeed = 2f;//How lazy the drift is (Higher = slower)
        [SerializeField] private float m_MovementThreshold = 0.1f;//Only reset if moving faster than this
        //Only resets if you're moving the mouse outside of this threshold
        [SerializeField] private float m_AimThreshold = 0.1f;
        
        private CinemachineThirdPersonFollow m_ThirdPersonComponent;
        private Vector3 m_FollowVelocity;
        private Vector2 m_CamAngles;
        
        private float m_TargetDistance;
        private float m_CurrentDistance;
        private float m_ZoomVelocity;
        
        private bool m_AutoAim;
        private float m_AutoRotateVelocity;
        private Coroutine m_CAutoAim;
        private Rigidbody m_TankRb;

        private void Awake()
        {
            if (!m_TankToFollow) 
            {
                Debug.Log("Error: Camera not given an object to follow!");
                return;
                
            }
            m_TankRb = m_TankToFollow.gameObject.GetComponent<Rigidbody>();
            if (m_VirtualCamera)
            {
                //Grabs the component that handles distance
                m_ThirdPersonComponent = m_VirtualCamera.GetComponent<CinemachineThirdPersonFollow>();
                
                //Initialises the target distance based on where the camera currently is
                if (m_ThirdPersonComponent)
                {
                    //Uses the editor value for the default zoom
                    m_TargetDistance = m_ThirdPersonComponent.CameraDistance;
                    m_CurrentDistance = m_TargetDistance;//current and target are the same by default
                }
            }

            //Initialises the starting rotation so the mouse doesn't cause the camera to immediately snap
            Vector3 currentEuler = transform.rotation.eulerAngles;
            m_CamAngles.x = currentEuler.x;
            m_CamAngles.y = currentEuler.y;
        }
        
        private void LateUpdate()
        {
            HandleSoftFollow();
            HandleZoom();
            HandleAutoAim();
        }
        
        private void HandleAutoAim()
        {
            //Only runs if the delay has ended and the tank is moving
            if (!m_AutoAim) return;
            if (!m_TankRb  || m_TankRb.linearVelocity.magnitude < m_MovementThreshold) return;

            //The tank's back
            float targetYaw = m_TankToFollow.eulerAngles.y;

            //Slowly drifts to the tank's back - wraparound is handled automatically
            m_CamAngles.y = Mathf.SmoothDampAngle(m_CamAngles.y, targetYaw, ref m_AutoRotateVelocity, m_AutoAimSpeed);
            
            //Applies the rotation to the camera
            transform.rotation = Quaternion.Euler(m_CamAngles);
        }
        
        private void HandleSoftFollow()
        {
            if (!m_TankToFollow) return;//ensures the tank still exists and wasn't blown up

            //The where the camera should be in relation to the tank - how far away
            Vector3 targetPos = m_TankToFollow.position + m_FollowOffset;
            
            //if the camera is too far away from the tank
            if (Vector3.Distance(transform.position, targetPos) > m_MaxFollowDistance)
            {
                transform.position = targetPos;
                //Resets the velocity so the smoothing doesn't get messed up by teleporting the camera
                m_FollowVelocity = Vector3.zero;
            }
            else
            {
                //Uses the camera speed and the smooth time to control how fast the camera should follow and track the tank
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref m_FollowVelocity, m_FollowSpeed);
                //the velocity decreases as the camera moves closer to the position so it needs to be recalculated
                //ref is used here so the function can have persistent access to the data
                //it reads and writes to the variable so it can pickup where it left off when the function is next run
                //works like the out keyword but reads the data as well
            }
        }

        public void AimStart(Vector2 deltaPos)
        {
            //Acts as a dead-zone before any movement can be registered - don't want tiny mouse movements to make the camera jitter
            if (deltaPos.magnitude < m_AimThreshold) return;
            
            m_AutoAim = false;
            if (m_CAutoAim != null) StopCoroutine(m_CAutoAim);//prevents auto aim from starting
            
            //applies the camera sensitivity
            //don't want the sense to just be pure native mouse input - inconsistent results
            deltaPos *= m_LookSensitivity;
            
            //flips the y delta because Unity tilts down with positive and up with negative 
            deltaPos.y *= -1;

            //CamAngles.x = CamAngles.x + deltaPos.y because x rotation is up and down whereas y rotation is left and right
            //This prevents you from looking into the floor or looking directly upwards (breaking your neck)
            m_CamAngles.x = Mathf.Clamp(m_CamAngles.x + deltaPos.y, m_MinXAngleDeg, m_MaxXAngleDeg);

            //Ensures the camera loops around the player cleanly - 360 goes back to 1 - makes debugging easier than just relying on quaternions
            m_CamAngles.y = Mathf.Repeat(m_CamAngles.y + deltaPos.x, 360f);

            //Applies the new orbiting rotation - The CM cam will then match this rotation
            transform.rotation = Quaternion.Euler(m_CamAngles);
        }

        public void AimEnd()
        {
            if (m_CAutoAim != null) StopCoroutine(m_CAutoAim);//kill the existing timer
            m_CAutoAim = StartCoroutine(C_AimEndDelay(m_AutoAimStartDelay));//can't use ??= here because I stop the timer dynamically instead
        }

        IEnumerator C_AimEndDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_AutoAim = true;
        }

        private void HandleZoom()
        {
            //if the camera isn't in third person follow mode
            if (!m_ThirdPersonComponent) return;

            //This is run every frame so it animates correctly
            //The camera won't move until the target distance changes because it is the same as the current distance by default 
            //Smoothly interpolates the current distance to target distance
            m_CurrentDistance =
                Mathf.SmoothDamp(m_CurrentDistance, m_TargetDistance, ref m_ZoomVelocity, m_ZoomSmoothTime);

            //this follow component is used to manage where the camera sits behind the target
            //essentially works the same as the basic Camera.LocalPosition = new Vector3()
            m_ThirdPersonComponent.CameraDistance = m_CurrentDistance;
        }
        
        public void OnZoom(float scrollDelta)
        {
            if (scrollDelta == 0) return;//if no input
            
            //Adding because a higher value (scrolling up) should mean an increase in distance (zooming in) 
            m_TargetDistance += scrollDelta * m_ZoomSpeed;

            //Stops you from zooming infinitely
            m_TargetDistance = Mathf.Clamp(m_TargetDistance, m_MinDistance, m_MaxDistance);
        }
    }
}