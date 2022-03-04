using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
    public float maxDist = 0f;
    public float vertOffset = 0f;
	public int grabbedMask;
	public bool doHover = false;

	private struct hoverObj {
		public GameObject gameObject;
		public Vector3 origPos;
	}
	int isHoveringIndex = -1;
	List<hoverObj> hoverObjs = new List<hoverObj>();
	int tempLayer = -1;

    private Camera cam;
    // Start is called before the first frame update
    void Start() {
        cam = GetComponent<Camera>();
	}

    // Update is called once per frame
    void Update() {
        Ray rayInfo = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHitInfo;
        LayerMask mask = ~(1 << grabbedMask);          //probably used later, i did?

        if (Physics.Raycast(rayInfo, out rayHitInfo, maxDist, mask)) {
            mouseObject.position = rayHitInfo.point + Vector3.up * vertOffset;
			if (doHover) HoverManagement(rayHitInfo);

			if (Input.GetMouseButtonDown(0)) {
				//first check if we wanna do smt
				GameObject hitObj = rayHitInfo.collider.gameObject;
				if (hitObj.CompareTag("Interactable")) {
					{
						Card cardTest = hitObj.GetComponent<Card>();
						if (cardTest != null) {
							rayHitInfo.rigidbody.isKinematic = true;
							cardTest.transform.SetParent(mouseObject, true);
							cardTest.transform.position += Vector3.up * vertOffset;
							tempLayer = cardTest.gameObject.layer;
							cardTest.gameObject.layer = grabbedMask;
							return;
						}
					}
					{
						DeckManager deckTest = hitObj.GetComponent<DeckManager>();
						if (deckTest != null) {
							Transform card = deckTest.DrawCard();
							if (card != null) {
								card.GetComponent<Rigidbody>().isKinematic = true;
								card.SetParent(mouseObject, true);
								card.position += Vector3.up * vertOffset;
								tempLayer = card.gameObject.layer;
								card.gameObject.layer = grabbedMask;
							}
							return;
						}
					}
					{
						PressEventButton buttonTest = hitObj.GetComponent<PressEventButton>();
						if (buttonTest != null) {
							buttonTest.Press();
							return;
						}
					}
				}
			}
			//we should be holding something
			if (tempLayer != -1) {
				if (Input.GetMouseButtonUp(0)) {
					GameObject hitObj = rayHitInfo.collider.gameObject;
					//cardholder test
					if (hitObj.CompareTag("Interactable")) {
						CardHolder tempHolder;
						if (hitObj.TryGetComponent<CardHolder>(out tempHolder)) {
							if (tempHolder.PutCard(mouseObject.GetChild(1).GetComponent<Card>())) {
								//dont allow cards to be interactable when in holder (aka leave layer)
								//mouseObject.GetChild(1).gameObject.layer = tempLayer;
								tempLayer = -1;
								return;
							}
						}
					}
					//just drop the card otherwise
					mouseObject.GetChild(1).GetComponent<Rigidbody>().isKinematic = false;
					mouseObject.GetChild(1).gameObject.layer = tempLayer;
					tempLayer = -1;
					mouseObject.GetChild(1).SetParent(null, true);
				}
			}
        }
    }

	void HoverManagement(RaycastHit rayHitInfo) {
		//Can only dehover if you're hovering smth and if raycast has different output then saved input
		if (IsHovering() && rayHitInfo.transform.gameObject != hoverObjs[isHoveringIndex].gameObject) StartCoroutine(DeActivateHover());
		//Can only animate the hovering if you arent hovering smth, if layer is card, and if parent is the hand
		if (!IsHovering() && rayHitInfo.transform.gameObject.layer == 6 && rayHitInfo.transform.parent && rayHitInfo.transform.parent.CompareTag("Player Hand")) StartCoroutine(ActivateHover(rayHitInfo));
	}

	bool IsHovering() {
		return isHoveringIndex > -1;
	}

	IEnumerator ActivateHover(RaycastHit rayHitInfo) {
		bool tempLoop = false;		
		hoverObj ms;
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

		Vector3 targetPos = ms.origPos + ms.gameObject.transform.localRotation * new Vector3(0f, 0.066f, 0.1f);
		Vector3 hoverObjVel = Vector3.zero;
		tempLoop = true;

		while (tempLoop) {																					//if escapes through here then it's a "clean exit"
			if (!IsHovering() || IsHovering() && ms.gameObject != hoverObjs[isHoveringIndex].gameObject)    //if escapes through here then it's a "unclean exit"
				break;
			tempLoop = Vector3.Distance(ms.gameObject.transform.localPosition, targetPos) >= 0.01f;

			ms.gameObject.transform.localPosition = Vector3.SmoothDamp(ms.gameObject.transform.localPosition, targetPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return null;
		}

		if (!tempLoop)
			ms.gameObject.transform.localPosition = targetPos;

		//Debug.Log("ActivateHover: " + hoverObjOrigPos);
		//Debug.Log("AcHoverObjsAmt: " + hoverObjs.Count);
	}

	IEnumerator DeActivateHover() {
		hoverObj ms;
		ms.gameObject = hoverObjs[isHoveringIndex].gameObject;
		ms.origPos = hoverObjs[isHoveringIndex].origPos;
		isHoveringIndex = -1;

		Vector3 hoverObjVel = Vector3.zero;
		bool tempLoop = true;

		while (tempLoop) {
			if (IsHovering() && ms.gameObject == hoverObjs[isHoveringIndex].gameObject)
				break;
			tempLoop = Vector3.Distance(ms.gameObject.transform.localPosition, ms.origPos) >= 0.01f;

			ms.gameObject.transform.localPosition = Vector3.SmoothDamp(ms.gameObject.transform.localPosition, ms.origPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return null;
		}

		if (!tempLoop) {
			ms.gameObject.transform.localPosition = ms.origPos;
			//indexes can change if other DeActivator deletes it, only gets deleted if loop exited "cleanly" as "unclean" exits means it needs it
			int indexToRemove = hoverObjs.FindIndex(x => x.gameObject == ms.gameObject);
			hoverObjs.RemoveAt(indexToRemove);
			if (isHoveringIndex >= indexToRemove && IsHovering()) --isHoveringIndex;
		}

		//Debug.Log("DeActivateHover: " + targetPos);
		//Debug.Log("DeHoverObjsAmt: " + hoverObjs.Count);
	}

	IEnumerator AnimateDeHover() {
		isHovering = false;
		GameObject tempHoverObj = hoverObj;
		hoverObj = null;
		Vector3 tempHoverObjOrigPos = hoverObjOrigPos;
		hoverObjOrigPos = Vector3.zero;
		Vector3 tempHoverObjVel = hoverObjVel;
		tempHoverObjVel = Vector3.zero;
		int tempLayer = tempHoverObj.layer;
		tempHoverObj.layer = grabbedMask;	//ignored layer
		while (true) {
			if (Vector3.Distance(tempHoverObj.transform.localPosition, tempHoverObjOrigPos) < 0.0001f) {
				tempHoverObj.transform.localPosition = tempHoverObjOrigPos;
				//isHovering = false;
				//hoverObj = null;
				//hoverObjOrigPos = Vector3.zero;
				//return true;        //fully "dehovered" Obj
				break;
			}

			tempHoverObj.transform.localPosition = Vector3.SmoothDamp(tempHoverObj.transform.localPosition,
				tempHoverObjOrigPos, ref tempHoverObjVel, 0.1f, 2f, Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}

		tempHoverObj.layer = tempLayer;
	}
}
