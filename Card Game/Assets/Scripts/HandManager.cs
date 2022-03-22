using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Mouse input;
	public bool doHover = false;
	public bool doSplay = false;
	public float SplayDegree = 0f;
	public int splaySelectIndex = -1;
	private struct HoverObj {
		public GameObject gameObject;
		public Vector3 origPos;
	}
	int isHoveringIndex = -1;
	private List<HoverObj> hoverObjs = new List<HoverObj>();

	private void OnEnable() {
		if (input)
			input.hoverEvent += HoverManagement;
	}
	private void OnDisable() {
		if (input)
			input.hoverEvent -= HoverManagement;
	}

	void HoverManagement(RaycastHit rayHitInfo) {
		if (!doHover)	return;
		if (doSplay) TestSplay();

		//Can only dehover if you're hovering smth and if raycast has different output then saved input
		if (IsHovering() && rayHitInfo.transform.gameObject != hoverObjs[isHoveringIndex].gameObject)
			StartCoroutine(DeActivateHover());
		//Can only animate the hovering if you arent hovering smth, if layer is card, if parent is the hand, and if mouse has only one child (card pickup makes card goto hand)
		if (!IsHovering() && rayHitInfo.transform.gameObject.layer == 6
			&& rayHitInfo.transform.parent == transform && input.mouseObject.transform.childCount == 1)
				StartCoroutine(ActivateHover(rayHitInfo));
	}

	void TestSplay() {
		doSplay = false;

		int extraCards = 6;			//Keep it even or bad stuff happens
		int extraCardsHalf = extraCards / 2;
		int effectiveChildCount = transform.childCount + (splaySelectIndex >= 0 ? extraCards : 0);
		float temp = 0.5f * SplayDegree * (effectiveChildCount - 1);
		Vector3 start = -transform.forward;

        for (int i = 0; i < effectiveChildCount; ++i) {
			if (splaySelectIndex >= 0 && Mathf.Abs(i - splaySelectIndex - extraCardsHalf) < (extraCardsHalf + 1) && i != splaySelectIndex + extraCardsHalf)
				continue;

			int effectedCard = i;
			if (splaySelectIndex >= 0 && i == splaySelectIndex + extraCardsHalf) effectedCard -= extraCardsHalf;
			if (splaySelectIndex >= 0 && i > splaySelectIndex + extraCardsHalf) effectedCard -= extraCards;

			transform.GetChild(effectedCard).localPosition = start + Quaternion.AngleAxis(SplayDegree * i - temp, Vector3.up) * transform.forward;
			transform.GetChild(effectedCard).localRotation = Quaternion.Euler(0f, SplayDegree * i - temp, -10f);
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

		if (!tempLoop) {
			//indexes can change if other DeActivator deletes it, only gets deleted if loop exited "cleanly" as "unclean" exits means it needs it
			int indexToRemove = hoverObjs.FindIndex(x => x.gameObject == ms.gameObject);
			hoverObjs.RemoveAt(indexToRemove);
			if (isHoveringIndex >= indexToRemove && IsHovering()) --isHoveringIndex;
		}
	}
}
