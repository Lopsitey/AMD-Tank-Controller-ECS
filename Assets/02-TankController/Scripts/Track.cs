#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace _02_TankController.Scripts
{
    public class Track : MonoBehaviour
    {
        private List<Suspension> m_SuspensionArms;
        public float TractionPercent { get; private set; }

        private void Awake()
        {
            //Gets all the suspension
            m_SuspensionArms = GetComponentsInChildren<Suspension>().ToList();
        }

        private void FixedUpdate()
        {
            int groundedCount = 0;
            //Counts how many arms are currently grounded
            foreach (var arm in m_SuspensionArms)
            {
                if (arm.IsGrounded)
                    ++groundedCount;
            }

            //The overall traction - not calculated if no wheels are on the ground
            TractionPercent = 0;
            if (m_SuspensionArms.Count > 0)
                TractionPercent = Mathf.Clamp01((float)groundedCount / m_SuspensionArms.Count);
        }
    }
}