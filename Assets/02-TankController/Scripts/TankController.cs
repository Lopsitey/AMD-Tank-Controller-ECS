using System.Collections;
using UnityEngine;

namespace _02_TankController.Scripts
{
	public class TankController : MonoBehaviour
	{
		private enum MovementState
		{
			Stopped,
			Moving
		}

		private enum ZoomState
		{
			UnZoomed,
			Zoomed
		}
		
		[SerializeField] private float m_AimEndDelay;
		private bool m_Aiming = false;
		
		private void Awake()
		{
			//...
		}

		private void Start()
		{
			//...
		}

		public void AimStart(Vector2 delta)
		{
			m_Aiming = true;
		}

		public void AimEnd()
		{
			StartCoroutine(C_AimEndDelay(m_AimEndDelay));
		}

		IEnumerator C_AimEndDelay(float delay)
		{
			yield return new WaitForSeconds(delay);
			m_Aiming = false;
		}
	}
}