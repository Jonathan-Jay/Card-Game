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

	void HoverManagement(RaycastHit rayHitInfo) {
		//Can only dehover if you're hovering smth and if raycast has different output then saved input
		if (isHovering && rayHitInfo.transform.gameObject != hoverObj) StartCoroutine(DeActivateHover(rayHitInfo));
		//Can only animate the hovering if you arent hovering smth and if layer is card
		if (!isHovering && rayHitInfo.transform.gameObject.layer == 6) StartCoroutine(ActivateHover(rayHitInfo));
	}

	IEnumerator ActivateHover(RaycastHit rayHitInfo) {
		isHovering = true;
		hoverObj = rayHitInfo.transform.gameObject;
		hoverObjOrigPos = hoverObj.transform.localPosition;

		GameObject tempHoverObj = hoverObj;
		Vector3 tempHoverObjOffset = tempHoverObj.transform.localRotation * new Vector3(0f, 0.066f, 0.1f);
		Vector3 targetPos = hoverObjOrigPos + tempHoverObjOffset;
		Vector3 hoverObjVel = Vector3.zero;
		bool tempLoop = true;

        while (tempLoop) {
			if (tempHoverObj != hoverObj)
				break;
			targetPos = hoverObjOrigPos + tempHoverObjOffset;
			tempLoop = Vector3.Distance(tempHoverObj.transform.localPosition, targetPos) >= 0.01f;

			tempHoverObj.transform.localPosition = Vector3.SmoothDamp(tempHoverObj.transform.localPosition, targetPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return null;
		}

		if (!tempLoop)
			tempHoverObj.transform.localPosition = targetPos;

		//Debug.Log("ActivateHover: " + hoverObjOrigPos);
	}

	IEnumerator DeActivateHover(RaycastHit rayHitInfo) {
		GameObject tempHoverObj = hoverObj;
		Vector3 targetPos = hoverObjOrigPos;
		Vector3 hoverObjVel = Vector3.zero;
		bool tempLoop = true;

		isHovering = false;
		hoverObj = null;
		hoverObjOrigPos = Vector3.zero;

		while (tempLoop) {
			if (tempHoverObj == hoverObj)
				break;
			tempLoop = Vector3.Distance(tempHoverObj.transform.localPosition, targetPos) >= 0.01f;

			tempHoverObj.transform.localPosition = Vector3.SmoothDamp(tempHoverObj.transform.localPosition, targetPos, ref hoverObjVel, 0.1f, 2f, Time.deltaTime);

			yield return null;
		}

		if (!tempLoop)
			tempHoverObj.transform.localPosition = targetPos;
		else
			hoverObjOrigPos = targetPos;    //In process of dehovering object has been hovered again thus OriginalPos was set incorrectly

		//Debug.Log("DeActivateHover: " + targetPos);
	}
}
