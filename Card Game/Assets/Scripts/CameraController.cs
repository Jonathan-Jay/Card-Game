using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public int index {get; private set;} = 0;
	[SerializeField]	List<Transform> posOptions = new List<Transform>();
	[SerializeField]	float moveSpeed = 2f;
	[SerializeField]	float rotSpeed = 3f;
	Transform targetTrans;
	bool transitioning = true;

	void Start()
	{
		targetTrans = posOptions[index];
		transitioning = true;
		StartCoroutine("MoveCam");
	}

	public void IncrementIndex(bool loop) {
		if (++index >= posOptions.Count) {
			if (loop) {
				index = 0;
			}
			else {
				index = posOptions.Count - 1;
			}
		}
		Transition();
	}

	public void DecrementIndex(bool loop) {
		if (--index < 0) {
			if (loop) {
				index = posOptions.Count - 1;
			}
			else {
				index = 0;
			}
		}
		Transition();
	}

	void Transition() {
		targetTrans = posOptions[index];
		if (!transitioning)
		{
			transitioning = true;
			StartCoroutine("MoveCam");
		}
	}

	IEnumerator MoveCam() {
		while (transitioning) {
			yield return Card.eof;

			if (Vector3.Distance(transform.position, targetTrans.position) > 0.1f) {
				transform.position = Vector3.Lerp(transform.position,
					targetTrans.position, moveSpeed * Time.deltaTime);
			}
			else {
				transform.position = Vector3.MoveTowards(transform.position,
					targetTrans.position, 0.25f * Time.deltaTime);
			}

			if (Quaternion.Angle(transform.rotation, targetTrans.rotation) > rotSpeed) {
				transform.rotation = Quaternion.Lerp(transform.rotation,
					targetTrans.rotation, rotSpeed * Time.deltaTime);
			}
			else {
				transform.rotation = Quaternion.RotateTowards(transform.rotation,
					targetTrans.rotation, rotSpeed * 2.5f * Time.deltaTime);
			}

			if (transform.position == targetTrans.position
				&& transform.rotation == targetTrans.rotation)
			{
				transform.position = targetTrans.position;
				transform.rotation = targetTrans.rotation;
				transitioning = false;
			}
		}
	}
}
