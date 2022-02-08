using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
	[SerializeField]	Transform target;
	[SerializeField]	Transform dirtyCheck;
	Vector3 lastPos = Vector3.zero;

    // Update is called once per frame
    void LateUpdate() {
		if (lastPos != dirtyCheck.position) {
        	transform.LookAt(target);
			transform.localRotation = transform.localRotation * Quaternion.Euler(0f, 180f, 0f);
			lastPos = dirtyCheck.position;
		}
    }
}
