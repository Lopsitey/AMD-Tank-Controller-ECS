#region

using System.Collections;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class SphereDebug : MonoBehaviour
    {
        private Rigidbody m_Rb;
        private bool m_CanWake = true;
        private Coroutine m_WakeOnce;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody>();
            m_Rb.Sleep();
        }
        
        void Update()
        {
            if (m_CanWake && Time.time > 1)
                m_WakeOnce ??= StartCoroutine(Wake());//ensures the coroutine is only started once using the null-coalescing operator
        }

        private IEnumerator Wake()
        {
            print("woke");
            m_Rb.WakeUp();
            yield return new WaitForEndOfFrame();
            m_CanWake = false;
        }
        private void OnCollisionEnter(Collision other)
        {
            // ReSharper disable once GrammarMistakeInStringLiteral
            print(gameObject.name + " collided with: " + other.gameObject.name);
        }

        private void OnCollisionExit(Collision other)
        {
            // ReSharper disable once GrammarMistakeInStringLiteral
            print(gameObject.name + " collided with: " + other.gameObject.name);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = m_Rb.IsSleeping() ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up);
        }
    }
}