#region

using System.Collections;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class CameraController : MonoBehaviour
    {
        private enum ZoomState
        {
            UnZoomed,
            Zoomed
        }

        [Header("Camera")] [SerializeField] private float m_AimEndDelay = 0.25f;
        [SerializeField] private float m_MinXAngleDeg = 10;
        [SerializeField] private float m_MaxXAngleDeg = 60;

        [Header("Zoom")] [SerializeField] private float m_MinDistance = 5f; // Closest zoom
        [SerializeField] private float m_MaxDistance = 20f; // Furthest zoom
        [SerializeField] private float m_ZoomSpeed = 2f;
        [SerializeField] private float m_ZoomSmoothTime = 0.2f;
        
        private bool m_AutoAim = false;
        private Vector2 m_CamAngles;
        private Transform m_Camera;
        
        private float m_TargetDistance;
        private float m_CurrentDistance;
        private float m_ZoomVelocity;
        
        private void Awake()
        {
            m_Camera = transform.GetChild(0).transform;
            
            // Initialises the target distance based on where the camera currently is
            if (m_Camera)
            {
                m_CurrentDistance = -m_Camera.localPosition.z;
                m_TargetDistance = m_CurrentDistance;//current and target are the same by default
            }
            else
            {
                Debug.LogError("CameraController: The camera does not exist!");
            }
        }

        private void LateUpdate()
        {
            if (!m_Camera) return;

            //This is run every frame so it animates correctly
            //The camera won't move until the target distance changes because it is the same as the current distance by default 
            //Smoothly interpolates the current distance to target distance
            m_CurrentDistance = Mathf.SmoothDamp(m_CurrentDistance, m_TargetDistance, ref m_ZoomVelocity, m_ZoomSmoothTime);
            
            //Moves the camera relative to its parent
            m_Camera.localPosition = new Vector3(0, 0, -m_CurrentDistance);
            //negative m_CurrentDistance because the camera should always be behind the tank
        }
        
        public void OnZoom(float scrollDelta)
        {
            if (scrollDelta == 0) return;//if no input

            //Adding because a higher value (scrolling up) should mean an increase in distance (zooming in) 
            m_TargetDistance += scrollDelta * m_ZoomSpeed;

            //Stops you from zooming infinitely
            m_TargetDistance = Mathf.Clamp(m_TargetDistance, m_MinDistance, m_MaxDistance);
        }
        
        public void AimStart(Vector2 deltaPos)
        {
            m_AutoAim = false;

            //flips the y delta because Unity tilts down with positive and up with negative 
            deltaPos.y *= -1;

            //CamAngles.x = CamAngles.x + deltaPos.y because x rotation is up and down whereas y rotation is left and right
            //This prevents you from looking into the floor or looking directly upwards (breaking your neck)
            m_CamAngles.x = Mathf.Clamp(m_CamAngles.x + deltaPos.y, m_MinXAngleDeg, m_MaxXAngleDeg);

            //Ensures the camera loops around the player cleanly - 360 goes back to 1 - makes debugging easier than just relying on quaternions
            m_CamAngles.y = Mathf.Repeat(m_CamAngles.y + deltaPos.x, 360f);

            //Applies the new orbiting rotation
            transform.rotation = Quaternion.Euler(m_CamAngles);
        }

        public void AimEnd()
        {
            StartCoroutine(C_AimEndDelay(m_AimEndDelay));
        }

        IEnumerator C_AimEndDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_AutoAim = true;
        }
    }
}