using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StopGraphicRay : MonoBehaviour
{
	public Camera currentCam;
	public GraphicRaycaster ray;

    // Update is called once per frame
    void Update()
    {
        if (currentCam.enabled && !ray.enabled) {
			ray.enabled = true;
		}
		else if (!currentCam.enabled && ray.enabled) {
			ray.enabled = false;
		}
    }
}
