#region

using System.Collections;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace _02_TankController.Scripts
{
    public class TankController : MonoBehaviour
    {
        private enum MovementState
        {
            Stopped,
            Moving
        }

        private enum ZoomState
        {
            UnZoomed,
            Zoomed
        }

        [Header("Movement")] [SerializeField] private float m_TankSpeed = 10f;
        [SerializeField] private Transform[] m_TankWheels = new Transform[5];
        [Header("Aiming")] [SerializeField] private float m_AimEndDelay = 0.25f;

        private bool m_Aiming = false;
        private Rigidbody m_Rb;

        private void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
        }

        public void Accelerate(float acceleration)
        {
            Vector3 forward = transform.forward;
            //applies force to the individual wheels
            foreach (Transform wheel in m_TankWheels)
                m_Rb.AddForceAtPosition(acceleration * forward, wheel.position, ForceMode.Acceleration);
        }

        public void AimStart(Vector2 delta)
        {
            m_Aiming = true;
        }

        public void AimEnd()
        {
            StartCoroutine(C_AimEndDelay(m_AimEndDelay));
        }

        IEnumerator C_AimEndDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_Aiming = false;
        }
    }
}