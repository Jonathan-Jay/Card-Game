using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField]	List<Transform> posOptions = new List<Transform>();
	[SerializeField] float speed = 2f;
	Transform targetTrans;
	int index = 0;
	bool transitioning = true;

	void Start()
	{
		targetTrans = posOptions[index];
		transitioning = true;
		StartCoroutine("MoveCam");
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) {
			if (++index >= posOptions.Count) {
				index = 0;
			}
			targetTrans = posOptions[index];
			if (!transitioning) {
				transitioning = true;
				StartCoroutine("MoveCam");
			}
		}
        if (Input.GetKeyDown(KeyCode.S)) {
			if (--index < 0) {
				index = posOptions.Count - 1;
			}
			targetTrans = posOptions[index];
			if (!transitioning) {
				transitioning = true;
				StartCoroutine("MoveCam");
			}
		}
    }

	IEnumerator MoveCam() {
		while (transitioning) {
			yield return new WaitForEndOfFrame();

			transform.position = Vector3.Lerp(transform.position, targetTrans.position, speed * Time.deltaTime);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetTrans.rotation, speed * Time.deltaTime);

			if (Quaternion.Angle(transform.rotation, targetTrans.rotation) < 0.5f &&
				Vector3.Distance(transform.position, targetTrans.position) < 0.01f)
			{
				transform.position = targetTrans.position;
				transform.rotation = targetTrans.rotation;
				transitioning = false;
			}
		}
	}
}
