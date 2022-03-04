using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
	public HandManager hand;
    public float maxDist = 0f;
    public float vertOffset = 0f;
	public int ignoredLayer;
	int tempLayer = -1;
	public event System.Action<RaycastHit> hoverEvent;
	public event System.Action<RaycastHit> clickEvent;
	public event System.Action<RaycastHit> releaseEvent;

    private Camera cam;
    // Start is called before the first frame update
    void Start() {
        cam = GetComponent<Camera>();
	}

    // Update is called once per frame
    void Update() {
        Ray rayInfo = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHitInfo;

        if (Physics.Raycast(rayInfo, out rayHitInfo, maxDist, ~(1 << ignoredLayer))) {
            mouseObject.position = rayHitInfo.point + Vector3.up * vertOffset;
			hoverEvent?.Invoke(rayHitInfo);

			if (Input.GetMouseButtonDown(0)) {
				clickEvent?.Invoke(rayHitInfo);
				//first check if we wanna do smt
				GameObject hitObj = rayHitInfo.transform.gameObject;
				if (hitObj.CompareTag("Interactable")) {
					{
						Card cardTest = hitObj.GetComponent<Card>();
						if (cardTest != null) {
							rayHitInfo.rigidbody.isKinematic = true;
							cardTest.transform.SetParent(mouseObject, true);
							cardTest.transform.position += Vector3.up * vertOffset;
							tempLayer = cardTest.gameObject.layer;
							cardTest.gameObject.layer = ignoredLayer;
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
								card.gameObject.layer = ignoredLayer;
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
			if (Input.GetMouseButtonUp(0)) {
				releaseEvent?.Invoke(rayHitInfo);
				if (tempLayer != -1) {

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
