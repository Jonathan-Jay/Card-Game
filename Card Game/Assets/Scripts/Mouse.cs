using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
    public float maxDist = 0f;
    public float vertOffset = 0f;
	public int grabbedMask;
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
}
