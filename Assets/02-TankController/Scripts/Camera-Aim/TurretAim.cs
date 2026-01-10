using UnityEngine;

namespace _02_TankController.Scripts.Camera_Aim
{
    public class TurretAim : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_TurretMesh; // The part that spins Y
        [SerializeField] private Transform m_BarrelMesh; // The part that pitches X (Child of Turret)
        [SerializeField] private Camera m_MainCamera;    // Or reference your CinemachineBrain

        [Header("Settings")]
        [SerializeField] private float m_TurretRotateSpeed = 50f;
        [SerializeField] private float m_BarrelPitchSpeed = 40f;
        
        [Header("Constraints")]
        [Range(0, 180)] [SerializeField] private float m_MaxElevation = 25f;  // Up
        [Range(0, 180)] [SerializeField] private float m_MaxDepression = 10f; // Down

        private void Update()
        {
            if (!m_MainCamera || !m_TurretMesh || !m_BarrelMesh) return;

            // 1. Get the Aim Target (Infinite distance along camera view)
            // You could Raycast here if you want to aim at specific walls/enemies
            Vector3 aimTarget = m_MainCamera.transform.position + (m_MainCamera.transform.forward * 1000f);

            HandleTurretRotation(aimTarget);
            HandleBarrelPitch(aimTarget);
        }

        private void HandleTurretRotation(Vector3 targetPosition)
        {
            // Calculate direction from tank to target
            Vector3 targetDir = targetPosition - m_TurretMesh.position;

            // PROJECT: Flatten this vector onto the plane defined by the Tank's Up vector
            // This ensures the turret rotates parallel to the tank chassis, not world Up
            Vector3 flattenedDir = Vector3.ProjectOnPlane(targetDir, transform.up);

            // Avoid errors if direction is zero (e.g. looking straight up)
            if (flattenedDir.sqrMagnitude < 0.001f) return;

            // Create the rotation looking at the projected point, keeping the Tank's Up as "Up"
            Quaternion targetRotation = Quaternion.LookRotation(flattenedDir, transform.up);

            // Smoothly rotate towards that quaternion
            m_TurretMesh.rotation = Quaternion.RotateTowards(
                m_TurretMesh.rotation, 
                targetRotation, 
                m_TurretRotateSpeed * Time.deltaTime
            );
        }

        private void HandleBarrelPitch(Vector3 targetPosition)
        {
            // 1. Get target direction relative to the Turret's current orientation
            // This converts the world target into "Local Turret Space"
            Vector3 localTargetDir = m_TurretMesh.InverseTransformPoint(targetPosition);

            // 2. Flatten local X (we only care about Y (up) and Z (forward) for pitch)
            // Normalizing ensures we just get a direction vector
            Vector3 flattenedVec = new Vector3(0, localTargetDir.y, localTargetDir.z).normalized;

            // 3. Create a rotation from "Forward" to that "Flattened Vector" along the X axis
            Quaternion targetPitch = Quaternion.FromToRotation(Vector3.forward, flattenedVec);

            // 4. Clamp the rotation (Math-heavy part)
            // We convert the Quaternion to an angle to clamp it, then convert back
            // This is safer than Euler clamping which can flip at 180 degrees
            if (GetClampedPitch(targetPitch, out Quaternion clampedRotation))
            {
                m_BarrelMesh.localRotation = Quaternion.RotateTowards(
                    m_BarrelMesh.localRotation, 
                    clampedRotation, 
                    m_BarrelPitchSpeed * Time.deltaTime
                );
            }
        }

        // Helper to clamp quaternion rotation around X axis
        private bool GetClampedPitch(Quaternion inputRot, out Quaternion result)
        {
            // Extract the angle from the Quaternion (Pure Quaternion math)
            // Note: inputRot.x works because we constructed it purely around X axis using FromToRotation
            inputRot.ToAngleAxis(out float angle, out Vector3 axis);

            // Normalize angle to -180 to 180 range
            if (angle > 180) angle -= 360;

            // Check if the axis is flipped (pointing left instead of right)
            // If FromToRotation found a negative angle, it might flip the axis vector
            if (axis.x < 0) angle = -angle;

            // Clamp
            float clampedAngle = Mathf.Clamp(angle, -m_MaxDepression, m_MaxElevation);

            // Rebuild Quaternion
            result = Quaternion.AngleAxis(clampedAngle, Vector3.right);
            return true;
        }
    }
}