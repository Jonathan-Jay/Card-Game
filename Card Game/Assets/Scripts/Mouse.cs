using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
    public float maxDist = 0f;
    public float vertOffset = 0f;
	public int grabbedMask;
	public bool doHover = false;

	bool isHovering = false;
	GameObject hoverObj;
	Vector3 hoverObjOrigPos = Vector3.zero;
	Vector3 hoverObjVel = Vector3.zero;
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
			if (doHover) Hover(rayHitInfo);

			if (Input.GetMouseButtonDown(0)) {
				//first check if we wanna do smt
				GameObject hitObj = rayHitInfo.collider.gameObject;
				if (hitObj.CompareTag("Interactable")) {
					Card cardTest;
					if (hitObj.TryGetComponent<Card>(out cardTest)) {
						rayHitInfo.rigidbody.isKinematic = true;
						cardTest.transform.SetParent(mouseObject, true);
						cardTest.transform.position += Vector3.up * vertOffset;
						tempLayer = cardTest.gameObject.layer;
						cardTest.gameObject.layer = grabbedMask;
						return;
					}
					DeckManager deckTest;
					if (hitObj.TryGetComponent<DeckManager>(out deckTest)) {
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
					PressEventButton buttonTest;
					if (hitObj.TryGetComponent<PressEventButton>(out buttonTest)) {
						buttonTest.Press();
						return;
					}
				}
			}
			//we should be holding something
			else if (tempLayer != -1) {
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

	bool Hover(RaycastHit rayHitInfo, bool tryDehover = true) {
		if (tryDehover && !DeHover(rayHitInfo)) return false;			//You cannot hover an object when dehovering another object
		if (rayHitInfo.transform.gameObject.layer != 6) return false;	//Dont hover a non-card
		if (!isHovering) {
			isHovering = true;
			hoverObj = rayHitInfo.transform.gameObject;
			hoverObjOrigPos = hoverObj.transform.position;
		}

		Vector3 targetPos = hoverObjOrigPos + hoverObj.transform.up * 0.1f;
		hoverObj.transform.position = Vector3.SmoothDamp(hoverObj.transform.position, targetPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

		return true;	//I am hover boi
	}

	bool DeHover(RaycastHit rayHitInfo) {
		if (!isHovering || rayHitInfo.transform.gameObject == hoverObj) return true;	//No Obj was previously hovered OR still hovering over same Obj

		if (Vector3.Distance(hoverObj.transform.position, hoverObjOrigPos) < 0.0001f ) {
			hoverObj.transform.position = hoverObjOrigPos;
			isHovering = false;
			hoverObj = null;
			hoverObjOrigPos = Vector3.zero;
			return true;		//fully "dehovered" Obj
		}

		hoverObj.transform.position = Vector3.SmoothDamp(hoverObj.transform.position, hoverObjOrigPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);
		
		return false;		//In process of "dehovering" Obj
	}
}
