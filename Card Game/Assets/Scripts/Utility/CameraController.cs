using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public int index = 0;
	[SerializeField]	List<Transform> posOptions = new List<Transform>();
	public float moveSpeed = 2f;
	public float rotSpeed = 3f;
	Transform targetTrans;
	public bool transitioning { get; private set;} = true;

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

	public void ForceTransition(Transform target) {
		if (target)
			targetTrans = target;
		else
			targetTrans = posOptions[index];
			
		if (!transitioning)
			StartCoroutine(MoveCam());
	}

	//for it to snap into position
	public void Snap() {
		transform.position = targetTrans.position;
		transform.rotation = targetTrans.rotation;
	}

	public Transform GetCurrent() {
		return posOptions[index];
	}

	void Transition() {
		targetTrans = posOptions[index];
		if (!transitioning)
			StartCoroutine(MoveCam());
	}

	IEnumerator MoveCam() {
		transitioning = true;
		while (transitioning) {
			yield return Card.eof;

			if (Vector3.Distance(transform.position, targetTrans.position) > moveSpeed * 0.025f) {
				transform.position = Vector3.Lerp(transform.position,
					targetTrans.position, moveSpeed * Time.deltaTime);
			}
			else if (transform.position != targetTrans.position) {
				transform.position = Vector3.MoveTowards(transform.position,
					targetTrans.position, 0.25f * Time.deltaTime);
				if (Vector3.Distance(transform.position, targetTrans.position) < 0.001f)
					transform.position = targetTrans.position;
			}

			if (Quaternion.Angle(transform.rotation, targetTrans.rotation) > rotSpeed) {
				transform.rotation = Quaternion.Lerp(transform.rotation,
					targetTrans.rotation, rotSpeed * Time.deltaTime);
			}
			else if (transform.rotation != targetTrans.rotation) {
				transform.rotation = Quaternion.RotateTowards(transform.rotation,
					targetTrans.rotation, rotSpeed * 2.5f * Time.deltaTime);
				if (Quaternion.Angle(transform.rotation, targetTrans.rotation) < 0.01f)
					transform.rotation = targetTrans.rotation;
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
