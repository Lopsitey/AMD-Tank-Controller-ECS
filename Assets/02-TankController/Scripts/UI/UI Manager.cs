#region

using System;
using System.Linq;
using _02_TankController.Scripts.Camera_Aim;
using _02_TankController.Scripts.Combat;
using _02_TankController.Scripts.Combat.Ammo;
using _02_TankController.Scripts.Wheel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace _02_TankController.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Document")] [SerializeField]
        private UIDocument m_UIDoc;

        [Header("Tank")] [SerializeField] private WheelManager m_TankWheelManager;
        [SerializeField] private TankShooting m_TankShooting;
        [SerializeField] private AmmoPool m_AmmoPool;

        [Header("Turret")] [SerializeField] private TurretAim m_TurretAim;
        [SerializeField] private float m_IconStartRotation = 180f;

        [SerializeField, HideInInspector] private float m_TankSpeed;
        [SerializeField, HideInInspector] private float m_TankRevs;
        [SerializeField, HideInInspector] private string m_AmmoTotal;
        [SerializeField, HideInInspector] private int m_BulletType;


        private VisualElement m_UIRoot;
        private ProgressBar m_SpeedBar;
        private ProgressBar m_RevBar;
        private Label m_AmmoLabel;
        private VisualElement m_TankIcon;
        private VisualElement[] m_AmmoTypes;

        private int m_CurrentAmmoCount;
        private int m_ClipSize;
        private int m_OldAmmoCount;

        public void Awake()
        {
            if (!m_UIDoc)
            {
                Debug.LogError("No UIDoc found on the UI manager");
                return;
            }

            m_UIRoot = m_UIDoc.rootVisualElement;

            if (!m_TankWheelManager)
            {
                Debug.LogError("No wheel manager found on the UI manager");
                return;
            }

            if (!m_TurretAim)
            {
                Debug.LogError("No turret manager found on the UI manager");
                return;
            }

            if (!m_TankShooting)
            {
                Debug.LogError("No tank shooting manager found on the UI manager");
                return;
            }

            m_SpeedBar = m_UIRoot.Q<ProgressBar>("Speed-Bar");
            m_RevBar = m_UIRoot.Q<ProgressBar>("Rev-Bar");
            m_AmmoLabel = m_UIRoot.Q<Label>("Ammo-Label");
            m_TankIcon = m_UIRoot.Q<VisualElement>("Tank-Icon");
            m_AmmoTypes = m_UIRoot.Q<VisualElement>("Ammo-Types-Container").Children().ToArray();

            DataBinding speedBinding = new DataBinding
            {
                dataSource = this,
                dataSourcePath = new PropertyPath(nameof(m_TankSpeed)),
                bindingMode = BindingMode.ToTarget, //one way or two-way
                updateTrigger = BindingUpdateTrigger.OnSourceChanged //when it updates
            };

            //Converts m/s to mph
            float maxSpeedMph = Mathf.Round(m_TankWheelManager.m_MaxSpeed * 2.237f);

            //Converts the float to the ProgressBar's expected range (usually 0 to 100)
            //Ref allows the variable to be modified by the function and accessed directly
            speedBinding.sourceToUiConverters.AddConverter((ref float speedVal) =>
                (speedVal / maxSpeedMph) * 100);

            //Binds to the inner bar length
            m_SpeedBar.SetBinding("value", speedBinding);

            DataBinding speedTitleBinding = new DataBinding
            {
                dataSource = this,
                dataSourcePath = new PropertyPath(nameof(m_TankSpeed)),
                bindingMode = BindingMode.ToTarget, //one way or two-way
                updateTrigger = BindingUpdateTrigger.OnSourceChanged //when it updates
            };

            // Convert the float "15" into the string "15 MPH"
            speedTitleBinding.sourceToUiConverters.AddConverter((ref float speedVal) =>
                $"{speedVal} MPH");

            m_SpeedBar.SetBinding("title", speedTitleBinding);

            DataBinding revBinding = new DataBinding
            {
                dataSource = this,
                dataSourcePath = new PropertyPath(nameof(m_TankRevs)),
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged
            };

            // Abs ensures the bar fills even when reversing
            revBinding.sourceToUiConverters.AddConverter((ref float revValue) =>
                Mathf.Abs(revValue) * 100f);

            m_RevBar.SetBinding("value", revBinding);

            m_AmmoLabel.SetBinding("text", new DataBinding
            {
                dataSource = this,
                dataSourcePath = new PropertyPath(nameof(m_AmmoTotal)),
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged
            });

            //iterates through each ammo type icon to set up individual bindings
            for (int i = 0; i < m_AmmoTypes.Length; i++)
            {
                int index = i; //counters loop closure issues
                //without the local copy, all bindings would reference the final value of i after the loop ends 
                var typeIcon = m_AmmoTypes[i];

                var binding = new DataBinding
                {
                    dataSource = this,
                    dataSourcePath = new PropertyPath(nameof(m_BulletType)),
                    bindingMode = BindingMode.ToTarget,
                };

                //source to UI converter is the only reason this binding exists
                //it doesn't actually bind any data, just uses the selected ammo type to toggle the selection class
                binding.sourceToUiConverters.AddConverter((ref int selectedType) =>
                {
                    //if the index of the icon is the same as the bullet type passed into the binding
                    bool isSelected = (selectedType == index);

                    //toggles selection on the iterated icon, when selected
                    typeIcon.EnableInClassList("ammo-type-selected", isSelected);
                    typeIcon.EnableInClassList("ammo-type", !isSelected);

                    //returns a string to make the bogus binding happy
                    return isSelected ? "Selected" : "";
                });

                //Binds to a dummy property to make the converter work
                typeIcon.SetBinding("tooltip", binding);
            }
        }

        private void FixedUpdate()
        {
            m_TankSpeed = Mathf.Abs(m_TankWheelManager.ForwardSpeed);
            m_TankSpeed = Mathf.Round(m_TankSpeed * 2.237f);

            m_TankRevs = (float)Math.Round(m_TankWheelManager.CurrentRevs, 2);

            BulletType bt = m_TankShooting.CurrentType;
            m_BulletType = (int)bt;
            m_CurrentAmmoCount = m_AmmoPool.Pools[bt].Count;
            m_ClipSize = m_AmmoPool.PoolLimits[bt];

            if (m_CurrentAmmoCount != m_OldAmmoCount)
            {
                m_OldAmmoCount = m_CurrentAmmoCount;
                m_AmmoTotal = $"{m_CurrentAmmoCount} / {m_ClipSize}";
            }

            //creates a new angle from the starting angle's degrees plus the turret's current orientation degrees
            Angle newAngle = new Angle(m_IconStartRotation + m_TurretAim.OrientAngle, AngleUnit.Degree);

            //rotates using the current angle (in euler degrees - could use radians if converted)
            m_TankIcon.style.rotate = new Rotate(newAngle);
            //rotate represents the CSS rotate function here
        }
    }
}