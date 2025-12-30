#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class Track : MonoBehaviour
    {
        //This is for debug purposes
        [SerializeField] private List<Suspension> m_SuspensionArms;

        private int m_TractionCounter;
        public float TractionPercent { get; private set; }

        private void Awake()
        {
            //Gets all the suspension
            m_SuspensionArms = GetComponentsInChildren<Suspension>().ToList();
        }

        private void Start()
        {
            foreach(var arm in m_SuspensionArms)
            {
                //subscribes to the individual events
                arm.Grounded += OnGround;
                arm.Airborne += InAir;
            }
        }

        void OnDestroy()
        {
            //unsubscribes on death to prevent memory leaks
            foreach(var arm in m_SuspensionArms)
            {
                arm.Grounded -= OnGround;
                arm.Airborne -= InAir;
            }
        }
        private void InAir()
        {
            //Decrements for wheels in the air
            m_TractionCounter -= 1;
        }

        private void OnGround() //todo use events from the suspension classes to report back if they are grounded or not
        {
            //Increments for grounded hits
            ++m_TractionCounter;
        }

        private void FixedUpdate()
        {
            // Count how many arms are currently grounded
            //int groundedCount = m_SuspensionArms.Count(arm => arm.transform.position.y > 0);
            
            //The overall traction - not calculated if no wheels are on the ground
            TractionPercent = m_TractionCounter <= 0
                ? 0.0f
                : Mathf.Clamp01(m_TractionCounter / (float)m_SuspensionArms.Count);
            Debug.Log(TractionPercent);
        }
    }
}