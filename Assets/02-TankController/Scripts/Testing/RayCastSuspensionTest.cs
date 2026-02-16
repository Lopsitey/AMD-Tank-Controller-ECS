using UnityEngine;

namespace _02_TankController.Scripts.Testing
{
    public class RayCastSuspensionTest : MonoBehaviour
    {
        [SerializeField] float m_SpringLength = 1f;
        [SerializeField] float m_WheelRadius = 1f;

        private Transform m_Wheel;
        private bool m_HitGround;
        private void Awake()
        {
            GetComponent<Rigidbody>();
            m_Wheel = transform.GetChild(0);
        }
        
        private void FixedUpdate()
        {
            Vector3 pos = transform.position;
            Vector3 dir = -transform.up;

            m_HitGround = Physics.Raycast(pos, dir, out var hitInfo, m_SpringLength);
            
            //Should be uncompressed by default
            //If something is hit the length should be compressed to the hit distance from the origin
            float currentLen = (!m_HitGround) ? (m_SpringLength) : (hitInfo.distance);
            
            //How compressed (out of 1) the spring would be.
            //Calculated using the current length / (total length - wheel size)  
            float percentOfSpringLen = Mathf.Clamp01(currentLen / (m_SpringLength - m_WheelRadius));

            //Compression percent is just (1 - that)
            float compressionPercent = 1 - percentOfSpringLen;
            
            //Moves the wheel in the desired direction using its current location and the distance
            m_Wheel.position = pos + (dir * currentLen);
        }

        private void OnDrawGizmosSelected()
        {
            //turns red when the suspension is in use - grey when not
            Gizmos.color = m_HitGround ? Color.red : Color.grey;
            if (m_Wheel)
                Gizmos.DrawLine(transform.position, m_Wheel.position);
        }
    }
}
