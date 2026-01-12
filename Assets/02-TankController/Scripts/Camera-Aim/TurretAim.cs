#region

using UnityEngine;

#endregion

namespace _02_TankController.Scripts.Camera_Aim
{
    public class TurretAim : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform m_TurretMesh; // The part that spins Y

        [SerializeField] private Transform m_BarrelMesh; // The part that pitches X (Child of Turret)
        [SerializeField] private Camera m_MainCamera; // Or reference your CinemachineBrain

        [Header("Settings")] [SerializeField] private float m_TurretRotateSpeed = 50f;
        [SerializeField] private float m_BarrelPitchSpeed = 40f;

        [Header("Constraints")] [Range(0, 180)] [SerializeField]
        private float m_MaxElevation = 25f; // Up

        [Range(0, 180)] [SerializeField] private float m_MaxDepression = 10f; // Down
        
        //The Y rotation of the turret (0 to 360)
        [SerializeField, HideInInspector] public float OrientAngle => m_TurretMesh.localEulerAngles.y;

        private void Update()
        {
            if (!m_MainCamera || !m_TurretMesh || !m_BarrelMesh) return;

            // Get the Aim Target (Infinite distance along camera view)
            Vector3 aimTarget = m_MainCamera.transform.position + (m_MainCamera.transform.forward * 1000f);

            HandleTurretRotation(aimTarget);
            HandleBarrelPitch(aimTarget);
        }

        private void HandleTurretRotation(Vector3 targetPosition)
        {
            // Direction from tank to target
            Vector3 targetDir = targetPosition - m_TurretMesh.position;

            // Projects the turret direction onto the tank so it rotates parallel to the tank chassis, not world Up
            Vector3 flattenedDir = Vector3.ProjectOnPlane(targetDir, transform.up);

            // Avoid errors if direction is zero (e.g. looking straight up)
            if (flattenedDir.sqrMagnitude < 0.001f) return;

            // Creates a rotation looking at the projected point, keeping up the same
            Quaternion targetRotation = Quaternion.LookRotation(flattenedDir, transform.up);

            // Smoothly rotate towards that quaternion, incrementing by the rotation speed 
            m_TurretMesh.rotation = Quaternion.RotateTowards(
                m_TurretMesh.rotation,
                targetRotation,
                m_TurretRotateSpeed * Time.deltaTime
            );
        }

        private void HandleBarrelPitch(Vector3 targetPosition)
        {
            // Gets target direction relative to the parent (Turret) current orientation
            // This converts the world target into "Local Turret Space"
            Vector3 localTargetDir = m_TurretMesh.InverseTransformPoint(targetPosition);

            //Gets rid of the x and normalizes for just get a Y,Z direction vector
            //This is done because Y and Z are pitch, up/down and forward/backward
            Vector3 flattenedVec = new Vector3(0, localTargetDir.y, localTargetDir.z).normalized;

            // Rotates on the X axis using the forward direction using the pitch direction
            // Forward direction because it would be facing forward on the Z direction axis
            Quaternion targetPitch = Quaternion.FromToRotation(Vector3.forward, flattenedVec);

            // Converts the Quaternion to an angle to clamp it, then convert back
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
            // Converts to usable degrees
            // Rot x works because the pitch is an x rotation
            inputRot.ToAngleAxis(out float angle, out Vector3 axis);

            // Ensures the angle stays below 180
            if (angle > 180) angle -= 360;

            // Ensures the angle is always on the right axis, by flipping any negative (left axis) angles
            // Left and right axis means X axis
            if (axis.x < 0) angle = -angle;

            float clampedAngle = Mathf.Clamp(angle, -m_MaxDepression, m_MaxElevation);

            // Convert back to a Quaternion
            result = Quaternion.AngleAxis(clampedAngle, Vector3.right);
            return true;
        }
    }
}