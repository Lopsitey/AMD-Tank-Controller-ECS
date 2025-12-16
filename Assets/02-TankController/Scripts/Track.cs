using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using _02_TankController.Scripts;

public class Track : MonoBehaviour
{
    //This is serialised just for debug purposes. You probably want to
    //hide this in the inspector.
    [SerializeField] private List<SuspensionArm> m_suspensionArms;
     
    //The percentage of how much traction this track has.
    private float m_tractionPercent = 0.0f;
     
    //The public field for the above variable
    public float TractionPercent { get => m_tractionPercent; }
     
    private void Awake()
    {
        //Get every suspension underneath this track object
        m_suspensionArms = GetComponentsInChildren<SuspensionArm>().ToList();   
    }
     
    void Update()
    {
        //Note: Update() is out of sync with SuspensionArm's FixedUpdate(). Consider
        //how you might fix this: one way would be via events, mentioned in the tutorial.
        //..
     
        int tractionCounter = 0;
     
        //Add one if the arm is grounded, otherwise, 0
        foreach (SuspensionArm arm in m_suspensionArms)
            tractionCounter += arm.IsGrounded ? 1 : 0;
     
        //None in contact? Set directly to 0 to prevent divide by zero exceptions
        if (tractionCounter <= 0)
            m_tractionPercent = 0.0f;
     
        //Otherwise to set count / counter
        else
            m_tractionPercent = Mathf.Clamp01(tractionCounter / (float)m_suspensionArms.Count);
    }
}