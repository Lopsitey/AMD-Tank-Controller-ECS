using UnityEngine;
using UnityEngine.InputSystem;

namespace _02_TankController.Scripts
{
	public class InputHandler : MonoBehaviour
	{
		private AM_02Tank m_ActionMap; //input
	    [SerializeField] private CameraController m_CameraController;
	    private WheelManager m_WheelManager;
	    private bool m_Paused;
		
		private void Awake()
		{
			m_ActionMap = new AM_02Tank();
			m_WheelManager = GetComponentInChildren<WheelManager>();
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
			m_ActionMap.Default.Fire.canceled += Handle_FireCanceled;
			m_ActionMap.Default.Aim.performed += Handle_AimPerformed;
			m_ActionMap.Default.Aim.canceled += Handle_AimCanceled;
			m_ActionMap.Default.Zoom.performed += Handle_ZoomPerformed;
			m_ActionMap.Default.Pause.performed += Handle_PausePerformed;
		}
		private void OnDisable()
		{
			m_ActionMap.Disable();

			m_ActionMap.Default.Accelerate.performed -= Handle_AcceleratePerformed;
			m_ActionMap.Default.Accelerate.canceled -= Handle_AccelerateCanceled;
			m_ActionMap.Default.Steer.performed -= Handle_SteerPerformed;
			m_ActionMap.Default.Steer.canceled -= Handle_SteerCanceled;
			m_ActionMap.Default.Fire.performed -= Handle_FirePerformed;
			m_ActionMap.Default.Fire.canceled -= Handle_FireCanceled;
			m_ActionMap.Default.Aim.performed -= Handle_AimPerformed;
			m_ActionMap.Default.Aim.canceled -= Handle_AimCanceled;
			m_ActionMap.Default.Zoom.performed -= Handle_ZoomPerformed;
			m_ActionMap.Default.Pause.performed -= Handle_PausePerformed;
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
			//starts applying acceleration to the vehicle
			if(m_WheelManager)
				m_WheelManager.StartAccelerate(context.ReadValue<float>());
		}

		private void Handle_AccelerateCanceled(InputAction.CallbackContext context)
		{
			//stops applying acceleration to the vehicle
			if (m_WheelManager)
				m_WheelManager.EndAccelerate();
		}

		private void Handle_SteerPerformed(InputAction.CallbackContext context)
		{
			//starts applying turning to the vehicle
			if(m_WheelManager)
				m_WheelManager.StartTurn(context.ReadValue<float>());
		}

		private void Handle_SteerCanceled(InputAction.CallbackContext context)
		{
			//stops applying turning to the vehicle
			if (m_WheelManager)
				m_WheelManager.EndTurn();
		}

		private void Handle_FirePerformed(InputAction.CallbackContext context)
		{

		}

		private void Handle_FireCanceled(InputAction.CallbackContext context)
		{

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
	}
}