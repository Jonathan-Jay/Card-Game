using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeypressCamController : MonoBehaviour
{
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
		if (ignore)	return;

		//mouse position check?
		if (scrollDelay > 0f) {
			scrollDelay -= Time.deltaTime;
		}

		if (Input.GetKeyDown(KeyCode.W) || (scrollCounter <= 0f && Input.mouseScrollDelta.y > 0f)) {
			cam.IncrementIndex(looping);
			scrollCounter = scrollDelay;
		}
		if (Input.GetKeyDown(KeyCode.S) || (scrollCounter <= 0f && Input.mouseScrollDelta.y < 0f)) {
			cam.DecrementIndex(looping);
			scrollCounter = scrollDelay;
		}
	}
}
