using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeypressCamController : MonoBehaviour
{
	[SerializeField] UnityEngine.InputSystem.InputAction navigate;
	public CameraController cam;
	public bool looping = false;

	bool ignore = false;

	public void IgnoreInput(bool val) {
		ignore = val;
	}
	
	public float scrollDelay = 0.1f;
	float scrollCounter = 0f;
	void Update()
	{
		//mouse position check?
		if (scrollCounter > 0f) {
			scrollCounter -= Time.deltaTime;
		}
	}

	private void Awake() {
		navigate.performed += ctx => {
			if (ignore)	return;

			float val = ctx.ReadValue<float>();

			if (scrollCounter <= 0f && val > 0.5f) {
				cam.IncrementIndex(looping);
				scrollCounter = scrollDelay;
			}
			if (scrollCounter <= 0f && val < -0.5f) {
				cam.DecrementIndex(looping);
				scrollCounter = scrollDelay;
			}
		};
	}

	private void OnEnable() {
		navigate.Enable();
	}

	private void OnDisable() {
		navigate.Disable();
	}
}
