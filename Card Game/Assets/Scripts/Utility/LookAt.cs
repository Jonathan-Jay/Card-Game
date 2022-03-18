using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
	public bool autoAssignCamera = false;
	static public System.Action ForceUpdateCamera;
	[SerializeField]	Transform target;
	[SerializeField]	Transform dirtyCheck;
	Vector3 lastPos = Vector3.zero;

	private void Start() {
		if (autoAssignCamera) {
			target = Camera.main.transform;
			dirtyCheck = Camera.main.transform;
		}
	}

	private void OnEnable() {
		if (autoAssignCamera)
			ForceUpdateCamera += UpdateCam;
	}

	private void OnDisable() {
		if (autoAssignCamera)
			ForceUpdateCamera -= UpdateCam;
	}

    // Update is called once per frame
    void LateUpdate() {
		if (lastPos != dirtyCheck.position) {
        	transform.LookAt(target);
			transform.localRotation = transform.localRotation * Quaternion.Euler(0f, 180f, 0f);
			lastPos = dirtyCheck.position;
		}
    }

	void UpdateCam() {
		//update the target
		target = Camera.main.transform;
		dirtyCheck = Camera.main.transform;

		//perform the dirty
		transform.LookAt(target);
		transform.localRotation = transform.localRotation * Quaternion.Euler(0f, 180f, 0f);
		lastPos = dirtyCheck.position;
	}
}
