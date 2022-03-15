using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeypressCamController : MonoBehaviour
{
	public CameraController cam;
	public bool looping = false;
	
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
			cam.IncrementIndex(looping);
		}
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
			cam.DecrementIndex(looping);
		}
    }
}
