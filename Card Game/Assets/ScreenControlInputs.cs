using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenControlInputs : MonoBehaviour
{
	public KeyCode toggleWindowed = KeyCode.Backslash;

	static int windowedWidth = Screen.width;
	static int windowedHeight = Screen.height;

	void Update() {
		if (Input.GetKeyDown(toggleWindowed)) {
			if (Screen.fullScreen) {
				Screen.SetResolution(windowedWidth, windowedHeight, false);
			}
			else {
				windowedWidth = Screen.width;
				windowedHeight = Screen.height;

				Resolution temp = Screen.resolutions[Screen.resolutions.Length - 1];
				Screen.SetResolution(temp.width, temp.height, true);
			}
		}

		if (Input.GetKeyDown(KeyCode.Equals)) {
			AudioListener.pause = !AudioListener.pause;
		}
	}
}
