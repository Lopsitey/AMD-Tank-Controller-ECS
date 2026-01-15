using _02_TankController.Scripts.Camera_Aim;
using _02_TankController.Scripts.Combat;
using _02_TankController.Scripts.Wheel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _02_TankController.Scripts.Input
{
	public class InputHandler : MonoBehaviour
	{
		[Header("Camera")]
	    [SerializeField] private CameraController m_CameraController;
	    
	    private AM_02Tank m_ActionMap; //input
	    private WheelManager m_WheelManager;
	    private TankShooting m_TankShooting;
	    private bool m_Paused;
		
		private void Awake()
		{
			m_ActionMap = new AM_02Tank();
			m_WheelManager = GetComponent<WheelManager>();
			m_TankShooting = GetComponent<TankShooting>();
		}

		private void Start()
		{
			//Locks the cursor to the window and hides it by default
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		
		private void OnEnable()
		{
			m_ActionMap.Enable();

			m_ActionMap.Default.Accelerate.performed += Handle_AcceleratePerformed;
			m_ActionMap.Default.Accelerate.canceled += Handle_AccelerateCanceled;
			m_ActionMap.Default.Steer.performed += Handle_SteerPerformed;
			m_ActionMap.Default.Steer.canceled += Handle_SteerCanceled;
			m_ActionMap.Default.Fire.performed += Handle_FirePerformed;
			m_ActionMap.Default.Pause.performed += Handle_PausePerformed;
			m_ActionMap.Default.SwapAmmo.performed += Handle_SwapAmmoPerformed;
			
			if(!m_CameraController) return;
			m_ActionMap.Default.Aim.performed += Handle_AimPerformed;
			m_ActionMap.Default.Aim.canceled += Handle_AimCanceled;
			m_ActionMap.Default.Zoom.performed += Handle_ZoomPerformed;
			m_ActionMap.Default.AdvancedAim.started += Handle_AdvancedAimStarted;
			m_ActionMap.Default.AdvancedAim.canceled += Handle_AdvancedAimCanceled;
			m_ActionMap.Default.CameraMode.performed += Handle_CameraModePerformed;
		}

		private void OnDisable()
		{
			m_ActionMap.Disable();

			m_ActionMap.Default.Accelerate.performed -= Handle_AcceleratePerformed;
			m_ActionMap.Default.Accelerate.canceled -= Handle_AccelerateCanceled;
			m_ActionMap.Default.Steer.performed -= Handle_SteerPerformed;
			m_ActionMap.Default.Steer.canceled -= Handle_SteerCanceled;
			m_ActionMap.Default.Fire.performed -= Handle_FirePerformed;
			m_ActionMap.Default.Pause.performed -= Handle_PausePerformed;
			m_ActionMap.Default.SwapAmmo.performed -= Handle_SwapAmmoPerformed;
			
			if(!m_CameraController) return;
			m_ActionMap.Default.Aim.performed -= Handle_AimPerformed;
			m_ActionMap.Default.Aim.canceled -= Handle_AimCanceled;
			m_ActionMap.Default.Zoom.performed -= Handle_ZoomPerformed;
			m_ActionMap.Default.AdvancedAim.started -= Handle_AdvancedAimStarted;
			m_ActionMap.Default.AdvancedAim.canceled -= Handle_AdvancedAimCanceled;
			m_ActionMap.Default.CameraMode.performed -= Handle_CameraModePerformed;
		}

		/// <summary>
		///Toggles hiding and unhiding the cursor with the pause menu.
		/// </summary>
		/// <param name="obj"></param>
		private void Handle_PausePerformed(InputAction.CallbackContext obj)
		{
			m_Paused = !m_Paused;

			if (m_Paused)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		private void Handle_AcceleratePerformed(InputAction.CallbackContext context)
		{
			if (!m_WheelManager) return;
			float accelDir = context.ReadValue<float>();
			m_WheelManager.StartAccelerate(accelDir);
		}

		private void Handle_AccelerateCanceled(InputAction.CallbackContext context)
		{
			if (!m_WheelManager) return;
			m_WheelManager.EndAccelerate();
		}

		private void Handle_SteerPerformed(InputAction.CallbackContext context)
		{
			if (!m_WheelManager) return;
			float turnDir = context.ReadValue<float>();
			m_WheelManager.StartTurn(turnDir);
		}

		private void Handle_SteerCanceled(InputAction.CallbackContext context)
		{
			if (!m_WheelManager) return;
			m_WheelManager.EndTurn();
		}

		private void Handle_FirePerformed(InputAction.CallbackContext context)
		{
			if (!m_TankShooting) return;
			m_TankShooting.Fire();
		}

		private void Handle_AimPerformed(InputAction.CallbackContext context)
		{
			Vector2 deltaPos = context.ReadValue<Vector2>();
			m_CameraController.AimStart(deltaPos);
		}

		private void Handle_AimCanceled(InputAction.CallbackContext context)
		{
			m_CameraController.AimEnd();
		}
		
		private void Handle_ZoomPerformed(InputAction.CallbackContext context)
		{
			m_CameraController.OnZoom(context.ReadValue<float>());
		}
		
		private void Handle_AdvancedAimStarted(InputAction.CallbackContext obj)
		{
			m_CameraController.OnAdvancedAim();
		}

		private void Handle_AdvancedAimCanceled(InputAction.CallbackContext obj)
		{
			m_CameraController.OnAdvancedAimEnd();
		}
		
		private void Handle_CameraModePerformed(InputAction.CallbackContext obj)
		{
			m_CameraController.ToggleCamera();
		}
		private void Handle_SwapAmmoPerformed(InputAction.CallbackContext context)
		{
			if (!m_TankShooting) return;
			int swapDir = (int)context.ReadValue<float>();
			m_TankShooting.SwitchType(swapDir);
		}
	}
}