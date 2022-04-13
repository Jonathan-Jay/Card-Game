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
	
	bool checkScroll = true;
	void Update()
	{
		if (ignore)	return;

		//mouse position check?
		if (!checkScroll && Input.mouseScrollDelta.y == 0f) {
			checkScroll = true;
		}

		if (Input.GetKeyDown(KeyCode.W) || (checkScroll && Input.mouseScrollDelta.y > 0)) {
			cam.IncrementIndex(looping);
			checkScroll = false;
		}
		if (Input.GetKeyDown(KeyCode.S) || (checkScroll && Input.mouseScrollDelta.y < 0)) {
			cam.DecrementIndex(looping);
			checkScroll = false;
		}
	}
}
