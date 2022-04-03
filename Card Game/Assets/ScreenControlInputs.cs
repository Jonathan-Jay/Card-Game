using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenControlInputs : MonoBehaviour
{
	public KeyCode toggleWindowed = KeyCode.Backslash;

	void Update(){
		if (Input.GetKeyDown(toggleWindowed)) {
			Screen.fullScreen = !Screen.fullScreen;
		}

		if (Input.GetKeyDown(KeyCode.Equals)) {
			AudioListener.pause = !AudioListener.pause;
		}
	}
}
