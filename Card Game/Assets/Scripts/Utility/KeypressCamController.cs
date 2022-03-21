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
	
	void Update()
	{
		if (ignore)	return;

		if (Input.GetKeyDown(KeyCode.W)) {
			cam.IncrementIndex(looping);
		}
		if (Input.GetKeyDown(KeyCode.S)) {
			cam.DecrementIndex(looping);
		}
	}
}
