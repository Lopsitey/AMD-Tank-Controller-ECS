using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace _02_TankController.Scripts
{
    public class StartedCamImplementation : MonoBehaviour
    {
        [Header("Cinemachine Integration")]
        [SerializeField] private CinemachineCamera m_VirtualCamera; // Drag CM vcam1 here

        [Header("Follow Settings")]
        [SerializeField] private Transform m_TankToFollow; // Drag your Tank here
        [SerializeField] private float m_FollowSmoothTime = 0.1f;
        [SerializeField] private Vector3 m_FollowOffset = new Vector3(0, 1.5f, 0);

        [Header("Orbit / Aim")] 
        [SerializeField] private float m_MinXAngleDeg = -10; // Negative allows looking up
        [SerializeField] private float m_MaxXAngleDeg = 60;  // Looking down
        [SerializeField] private float m_AimEndDelay = 0.25f;

        [Header("Zoom Settings")]
        [SerializeField] private float m_MinDistance = 2f;
        [SerializeField] private float m_MaxDistance = 15f;
        [SerializeField] private float m_ZoomSpeed = 5f;
        [SerializeField] private float m_ZoomSmoothTime = 0.2f;

        // Internal State
        private Vector3 m_FollowVelocity;
        private Vector2 m_CamAngles;
        private float m_TargetDistance;
        private float m_CurrentDistance;
        private float m_ZoomVelocity;
        private bool m_AutoAim = true;
        
        // Cache the Cinemachine Component for performance
        private CinemachineThirdPersonFollow m_ThirdPersonComponent;

        private void Awake()
        {
            // 1. Detach Logic: Ensure this object is not a child of the tank
            transform.parent = null; 

            // 2. Setup Zoom Defaults
            if (m_VirtualCamera)
            {
                // Grab the specific component that handles distance
                //m_ThirdPersonComponent = m_VirtualCamera.GetCinemachineComponent<CinemachineThirdPersonFollow>();
                
                if (m_ThirdPersonComponent)
                {
                    // Start zoom at whatever the editor value is
                    m_TargetDistance = m_ThirdPersonComponent.CameraDistance;
                    m_CurrentDistance = m_TargetDistance;
                }
            }

            // 3. Initialize Angles
            Vector3 currentEuler = transform.rotation.eulerAngles;
            m_CamAngles.x = currentEuler.x;
            m_CamAngles.y = currentEuler.y;
        }

        private void LateUpdate()
        {
            HandleSoftFollow();
            HandleZoom();
        }

        // --- 1. SMOOTH FOLLOW (Removes Tank Jitter) ---
        private void HandleSoftFollow()
        {
            if (!m_TankToFollow) return;

            // Determine where the target *should* be
            Vector3 targetPos = m_TankToFollow.position + m_FollowOffset;

            // Smoothly move there. This isolates the camera from the tank's vibrations.
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref m_FollowVelocity, m_FollowSmoothTime);
        }

        // --- 2. AIMING (Your Custom Logic) ---
        // Call this from your Input Manager
        public void AimStart(Vector2 deltaPos)
        {
            m_AutoAim = false;
            
            // Standard Orbit Logic
            deltaPos.y *= -1; // Flip Y if needed
            m_CamAngles.x = Mathf.Clamp(m_CamAngles.x + deltaPos.y, m_MinXAngleDeg, m_MaxXAngleDeg);
            m_CamAngles.y = Mathf.Repeat(m_CamAngles.y + deltaPos.x, 360f);

            // Rotate THIS object (The Target). Cinemachine will rotate the camera to match.
            transform.rotation = Quaternion.Euler(m_CamAngles);
        }
        
        // --- 3. ZOOM (Pushing data to Cinemachine) ---
        // Call this from Input Manager
        public void OnZoom(float scrollDelta)
        {
             if (scrollDelta == 0) return;
             
             // Zoom In (Decrease Distance) / Zoom Out (Increase Distance)
             m_TargetDistance -= scrollDelta * m_ZoomSpeed;
             m_TargetDistance = Mathf.Clamp(m_TargetDistance, m_MinDistance, m_MaxDistance);
        }

        private void HandleZoom()
        {
            if (!m_ThirdPersonComponent) return;

            // Smooth the value
            m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, m_TargetDistance, ref m_ZoomVelocity, m_ZoomSmoothTime);

            // Apply directly to Cinemachine
            m_ThirdPersonComponent.CameraDistance = m_CurrentDistance;
        }
        
        // ... (AimEnd / AutoAim Logic remains the same) ...
        public void AimEnd() { StartCoroutine(C_AimEndDelay(m_AimEndDelay)); }
        IEnumerator C_AimEndDelay(float delay) { yield return new WaitForSeconds(delay); m_AutoAim = true; }
    }
}