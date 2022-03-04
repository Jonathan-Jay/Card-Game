using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
	public Mouse input;
	public bool doHover = false;
	private struct HoverObj
	{
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

	void HoverManagement(RaycastHit rayHitInfo)
	{
		if (!doHover)	return;

		//Can only dehover if you're hovering smth and if raycast has different output then saved input
		if (IsHovering() && rayHitInfo.transform.gameObject != hoverObjs[isHoveringIndex].gameObject)
			StartCoroutine(DeActivateHover());
		//Can only animate the hovering if you arent hovering smth, if layer is card, and if parent is the hand
		if (!IsHovering() && rayHitInfo.transform.gameObject.layer == 6
			&& rayHitInfo.transform.parent == transform)
				StartCoroutine(ActivateHover(rayHitInfo));
	}

	bool IsHovering()
	{
		return isHoveringIndex > -1;
	}

	IEnumerator ActivateHover(RaycastHit rayHitInfo)
	{
		bool tempLoop = false;
		HoverObj ms;
		ms.gameObject = rayHitInfo.transform.gameObject;
		ms.origPos = ms.gameObject.transform.localPosition;

		for (isHoveringIndex = 0; isHoveringIndex < hoverObjs.Count; ++isHoveringIndex)
			if (hoverObjs[isHoveringIndex].gameObject == ms.gameObject)
			{
				tempLoop = true;
				ms.origPos = hoverObjs[isHoveringIndex].origPos;
				break;
			}

		if (!tempLoop)
			hoverObjs.Add(ms);

		Vector3 targetPos = ms.origPos + Vector3.up * 0.066f;
		Vector3 hoverObjVel = Vector3.zero;
		tempLoop = true;

		while (tempLoop)
		{                                                                                   //if escapes through here then it's a "clean exit"
			if (!IsHovering() || IsHovering() && ms.gameObject != hoverObjs[isHoveringIndex].gameObject)    //if escapes through here then it's a "unclean exit"
				break;
			tempLoop = Vector3.Distance(ms.gameObject.transform.localPosition, targetPos) >= 0.01f;

			ms.gameObject.transform.localPosition = Vector3.SmoothDamp(ms.gameObject.transform.localPosition, targetPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return Card.eof;
		}

		if (!tempLoop)
			ms.gameObject.transform.localPosition = targetPos;

		//Debug.Log("ActivateHover: " + hoverObjOrigPos);
		//Debug.Log("AcHoverObjsAmt: " + hoverObjs.Count);
	}

	IEnumerator DeActivateHover()
	{
		HoverObj ms;
		ms.gameObject = hoverObjs[isHoveringIndex].gameObject;
		ms.origPos = hoverObjs[isHoveringIndex].origPos;
		isHoveringIndex = -1;

		Vector3 hoverObjVel = Vector3.zero;
		bool tempLoop = true;

		while (tempLoop)
		{
			if (IsHovering() && ms.gameObject == hoverObjs[isHoveringIndex].gameObject)
				break;
			tempLoop = Vector3.Distance(ms.gameObject.transform.localPosition, ms.origPos) >= 0.01f;

			ms.gameObject.transform.localPosition = Vector3.SmoothDamp(ms.gameObject.transform.localPosition, ms.origPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return Card.eof;
		}

		if (!tempLoop)
		{
			ms.gameObject.transform.localPosition = ms.origPos;
			//indexes can change if other DeActivator deletes it, only gets deleted if loop exited "cleanly" as "unclean" exits means it needs it
			int indexToRemove = hoverObjs.FindIndex(x => x.gameObject == ms.gameObject);
			hoverObjs.RemoveAt(indexToRemove);
			if (isHoveringIndex >= indexToRemove && IsHovering()) --isHoveringIndex;
		}

		//Debug.Log("DeActivateHover: " + targetPos);
		//Debug.Log("DeHoverObjsAmt: " + hoverObjs.Count);
	}
}
