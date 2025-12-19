
namespace _02_TankController.Scripts
{
    using UnityEngine;

    public class SuspensionArm : MonoBehaviour
    {
        [SerializeField] private float m_SuspensionArmLength = 0.5f;
        [SerializeField] private LayerMask m_SuspensionLayerMask;

        public bool IsGrounded { get; private set; }

        private float m_RaycastHitDist;
        private void FixedUpdate()
        {
            if(Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, m_SuspensionArmLength, m_SuspensionLayerMask))
            {
                //Hit something? This wheel is grounded
                m_RaycastHitDist = hit.distance;
                IsGrounded = true;
            }
            else
            {
                //Not hit something? This wheel is not grounded
                m_RaycastHitDist = 0;
                IsGrounded = false;
            }
        }
        private void OnDrawGizmos()
        {
            //Stops if in the editor
            if (!Application.isPlaying) 
                return;
            
            if (IsGrounded)
                Gizmos.color = Color.green;
            else 
                Gizmos.color = Color.red;

            //Draws the base/minimum detection line
            Vector3 endPoint = transform.position + -transform.up * m_RaycastHitDist;
            Gizmos.DrawLine(transform.position, endPoint);

            //Draws the remainder using the length of the suspension
            Gizmos.color = Color.black;
            Gizmos.DrawLine(endPoint, endPoint + -transform.up * (m_SuspensionArmLength - m_RaycastHitDist));
        }
    }

}
