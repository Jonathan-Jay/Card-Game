using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Mouse input;
	public bool doHover = false;
	public bool doSplay = false;
	public int splayMinimum = 3;
	public float totalSplayDegree = 90f;
	public float hoverHeight = 0.1f;
	public float cardTilt = 10f;
	float SplayDegree = 15f;
	public int splaySelectEmptiness = 3;
	public int splaySelectIndex = -1;

	private int splaySelectIndexOld = -1;
	private struct HoverObj {
		public GameObject gameObject;
		public Vector3 origPos;
	}
	
	int isHoveringIndex = -1;
	private List<HoverObj> hoverObjs = new List<HoverObj>();

	private void OnEnable() {
		input.hoverEvent += HoverManagement;
	}
	private void OnDisable() {
		input.hoverEvent -= HoverManagement;
	}

	void HoverManagement(RaycastHit rayHitInfo) {
		if (!doHover)	return;

		splaySelectIndexOld = splaySelectIndex;
		splaySelectIndex = (rayHitInfo.transform.gameObject.layer == 6 && rayHitInfo.transform.parent == transform && input.mouseObject.transform.childCount == 1) ? rayHitInfo.transform.GetSiblingIndex() : -1;

		doSplay = doSplay || splaySelectIndex != splaySelectIndexOld;
		if (doSplay) TestSplay();

		//Can only dehover if you're hovering smth and if raycast has different output then saved input
		/*if (IsHovering() && rayHitInfo.transform.gameObject != hoverObjs[isHoveringIndex].gameObject)
			StartCoroutine(DeActivateHover());
		//Can only animate the hovering if you arent hovering smth, if layer is card, if parent is the hand, and if mouse has only one child (card pickup makes card goto hand)
		if (!IsHovering() && rayHitInfo.transform.gameObject.layer == 6
			&& rayHitInfo.transform.parent == transform && input.mouseObject.transform.childCount == 1)
				StartCoroutine(ActivateHover(rayHitInfo));*/
	}

	void TestSplay() {
		doSplay = false;

		int effectiveChildCount = transform.childCount;
		int first = 0;
		int second = 0;
		float defaultTilt = cardTilt;

		if (splaySelectIndex >= 0 && transform.childCount > splayMinimum) {
			//make sure tilt is negative
			defaultTilt = -Mathf.Abs(defaultTilt);

			//if not left edge
			if (splaySelectIndex > 0) {
				first = splaySelectEmptiness;
			}
			second = first;
			//if not on right edge (adds to first so first is 0 if left edge)
			if (splaySelectIndex < transform.childCount - 1) {
				second += splaySelectEmptiness;
			}
			//only once if on the edge, twice if not
			effectiveChildCount += second;
		}

		SplayDegree = totalSplayDegree / effectiveChildCount;

		float temp = 0.5f * (effectiveChildCount - 1);

        for (int i = 0; i < transform.childCount; ++i) {

			int effectiveCard = i;
			Vector3 basePos = Vector3.back;
			float tilt = defaultTilt;
			if (splaySelectIndex >= 0){
				if (i == splaySelectIndex) {
					effectiveCard += first;
					basePos += Vector3.up * hoverHeight;
					tilt = 0f;
				}
				else if (i > splaySelectIndex) {
					effectiveCard += second;
					//invert tilt to help navigation
					tilt = -tilt;
				}
			}

			transform.GetChild(i).localPosition = basePos + Quaternion.AngleAxis(SplayDegree * (effectiveCard - temp), Vector3.up) * Vector3.forward;
			transform.GetChild(i).localRotation = Quaternion.Euler(0f, SplayDegree * (effectiveCard - temp), tilt);
		}
	}

	bool IsHovering() {
		return isHoveringIndex > -1;
	}

	IEnumerator ActivateHover(RaycastHit rayHitInfo) {
		bool tempLoop = false;
		HoverObj ms;
		ms.gameObject = rayHitInfo.transform.gameObject;
		ms.origPos = ms.gameObject.transform.localPosition;

		for (isHoveringIndex = 0; isHoveringIndex < hoverObjs.Count; ++isHoveringIndex)
			if (hoverObjs[isHoveringIndex].gameObject == ms.gameObject) {
				tempLoop = true;
				ms.origPos = hoverObjs[isHoveringIndex].origPos;
				break;
			}

		if (!tempLoop)
			hoverObjs.Add(ms);

		Vector3 targetPos = ms.origPos + Vector3.up * 0.066f;
		Vector3 hoverObjVel = Vector3.zero;
		tempLoop = true;

		//if escapes through here then it's a "clean exit"
		while (tempLoop) {
			if (!IsHovering() || IsHovering() && ms.gameObject != hoverObjs[isHoveringIndex].gameObject || ms.gameObject.transform.parent != transform)    //if escapes through here then it's a "unclean exit"
				break;
			tempLoop = Vector3.Distance(ms.gameObject.transform.localPosition, targetPos) >= 0.01f;

			ms.gameObject.transform.localPosition = Vector3.SmoothDamp(ms.gameObject.transform.localPosition, targetPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return Card.eof;
		}

		if (!tempLoop)
			ms.gameObject.transform.localPosition = targetPos;
	}

	IEnumerator DeActivateHover() {
		HoverObj ms;
		ms.gameObject = hoverObjs[isHoveringIndex].gameObject;
		ms.origPos = hoverObjs[isHoveringIndex].origPos;
		isHoveringIndex = -1;

		Vector3 hoverObjVel = Vector3.zero;
		bool tempLoop = true;

		while (tempLoop) {
			if (IsHovering() && ms.gameObject == hoverObjs[isHoveringIndex].gameObject || ms.gameObject.transform.parent != transform)
				break;
			tempLoop = Vector3.Distance(ms.gameObject.transform.localPosition, ms.origPos) >= 0.01f;

			ms.gameObject.transform.localPosition = Vector3.SmoothDamp(ms.gameObject.transform.localPosition, ms.origPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return Card.eof;
		}

		if (!tempLoop)
			ms.gameObject.transform.localPosition = ms.origPos;

		if (!tempLoop || ms.gameObject.transform.parent != transform) {
			//indexes can change if other DeActivator deletes it, only gets deleted if loop exited "cleanly" as "unclean" exits means it needs it
			int indexToRemove = hoverObjs.FindIndex(x => x.gameObject == ms.gameObject);
			hoverObjs.RemoveAt(indexToRemove);
			if (isHoveringIndex >= indexToRemove && IsHovering()) --isHoveringIndex;
		}
	}

	public void ReturnCardToHand(Transform cardTrans) {
		//copied this coroutine from the card class
		cardTrans.SetParent(transform, true);
		cardTrans.GetComponent<Rigidbody>().isKinematic = true;
		TestSplay();
		//StartCoroutine(ReturnToHand(cardTrans));
	}

	IEnumerator ReturnToHand(Transform card) {
		card.gameObject.layer = input.ignoredLayer;

		float returnSpeed = 2f;
		float returnRotSpeed = 135f;
		card.GetComponent<Rigidbody>().isKinematic = true;

		Vector3 targetPos = Vector3.zero;
		Quaternion targetRot = Quaternion.Euler(0f, 0f, 10f);

		while (card.parent == transform)
		{
			if (Vector3.Distance(card.localPosition, targetPos) > 0.1f) {
				card.localPosition = Vector3.Lerp(card.localPosition, targetPos,
					returnSpeed * Time.deltaTime);
			}
			else {
				card.localPosition = Vector3.MoveTowards(card.localPosition, targetPos,
					0.25f * Time.deltaTime);
			}

			if (card.localRotation != targetRot) {
			//if (Quaternion.Angle(transform.localRotation, targetRot) > 1f) {
			//	transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot,
			//		returnSpeed * Time.deltaTime);
			//}
			//else {
				card.localRotation = Quaternion.RotateTowards(card.localRotation, targetRot,
					returnRotSpeed * Time.deltaTime);
			}

			if (card.localPosition == targetPos && card.localRotation == targetRot){
				break;
			}
			yield return Card.eof;
		}

		//ensure transform is good
		if (card.parent == transform) {
			card.localPosition = targetPos;
			card.localRotation = targetRot;
		}

		card.gameObject.layer = input.cardLayer;
	}
}
