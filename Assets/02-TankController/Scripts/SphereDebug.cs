#region

using System.Collections;
using Mono.Collections.Generic;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class SphereDebug : MonoBehaviour
    {
        private Rigidbody m_RB;
        private bool m_canWake = true;
        Coroutine m_wakeOnce = null;

        void Awake()
        {
            m_RB = GetComponent<Rigidbody>();
            m_RB.Sleep();
        }
        
        void Update()
        {
            if (m_canWake && Time.time > 1)
                m_wakeOnce ??= StartCoroutine(Wake());
        }

        private IEnumerator Wake()
        {
            print("woke");
            m_RB.WakeUp();
            yield return new WaitForEndOfFrame();
            m_canWake = false;
        }
        private void OnCollisionEnter(Collision other)
        {
            print(gameObject.name + " collided with: " + other.gameObject.name);
        }

        private void OnCollisionExit(Collision other)
        {
            print(gameObject.name + " collided with: " + other.gameObject.name);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            if(m_RB.IsSleeping())
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            
            Gizmos.DrawLine(transform.position, transform.position + transform.up);
        }
    }
}