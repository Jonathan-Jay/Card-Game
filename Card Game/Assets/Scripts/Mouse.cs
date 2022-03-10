using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
	public PlayerData player;
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
    private void Awake() {
        cam = GetComponent<Camera>();
	}

	//delete this later to allow a networking manager to handle it
	private void Start() {
		LinkInteractablesFunc();
	}

	public Transform holding = null;

    // Update is called once per frame
    void Update() {
        Ray rayInfo = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHitInfo;

        if (Physics.Raycast(rayInfo, out rayHitInfo, maxDist, ~(1 << ignoredLayer))) {
            mouseObject.position = rayHitInfo.point + Vector3.up * vertOffset;
			
			hoverEvent?.Invoke(rayHitInfo);

			if (Input.GetMouseButtonDown(0)) {
				clickEvent?.Invoke(rayHitInfo);
			}
			//we should be holding something
			if (Input.GetMouseButtonUp(0)) {
				releaseEvent?.Invoke(rayHitInfo);
			}
        }
    }

	public void LinkInteractablesFunc() {
		//make sure we're not holding anything
		if (holding) {
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.gameObject.layer = tempLayer;
			holding.SetParent(null, true);
			holding = null;
		}
		clickEvent += ClickInteractable;
		releaseEvent += ReleaseInteractable;
	}

	public void UnLinkInteractablesFunc() {
		//make sure we're not holding anything
		if (holding) {
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.gameObject.layer = tempLayer;
			holding.SetParent(null, true);
			holding = null;
		}
		clickEvent -= ClickInteractable;
		releaseEvent -= ReleaseInteractable;
	}

	public void ActivateSpellMode() {
		UnLinkInteractablesFunc();
		//change rendering
		
	}

	public void DeactivateSpellMode() {
		LinkInteractablesFunc();
		//change rendering

	}

	public void ClickInteractable(RaycastHit hit) {
		//first check if we wanna do smt
		GameObject hitObj = hit.transform.gameObject;
		if (hitObj.CompareTag("Interactable")) {
			{
				Card cardTest = hitObj.GetComponent<Card>();
				if (cardTest != null && cardTest.player == player) {
					hit.rigidbody.isKinematic = true;
					cardTest.transform.SetParent(mouseObject, true);
					cardTest.transform.position += Vector3.up * vertOffset;
					tempLayer = cardTest.gameObject.layer;
					cardTest.gameObject.layer = ignoredLayer;

					holding = cardTest.transform;
					return;
				}
			}
			{
				DeckManager deckTest = hitObj.GetComponent<DeckManager>();
				if (deckTest != null && deckTest.player == player)
				{
					Transform card = deckTest.DrawCard();
					if (card != null)
					{
						card.GetComponent<Rigidbody>().isKinematic = true;
						card.SetParent(mouseObject, true);
						card.position += Vector3.up * vertOffset;
						tempLayer = card.gameObject.layer;
						card.gameObject.layer = ignoredLayer;

						holding = card;
					}
					return;
				}
			}
			{
				PressEventButton buttonTest = hitObj.GetComponent<PressEventButton>();
				if (buttonTest != null && (buttonTest.anyPlayer || buttonTest.player == player)) {
					buttonTest.Press();
					return;
				}
			}
		}
	}
	public void ReleaseInteractable(RaycastHit hit) {
		if (holding) {
			GameObject hitObj = hit.transform.gameObject;
			//cardholder test
			if (hitObj.CompareTag("Interactable")) {
				CardHolder tempHolder = hitObj.GetComponent<CardHolder>();
				if (tempHolder != null && tempHolder.PutCard(holding.GetComponent<Card>())) {
					//dont allow cards to be interactable when in holder (aka dont change the layer)
					//holding.gameObject.layer = tempLayer;
					holding = null;
					return;
				}
			}

			//just drop the card otherwise
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.gameObject.layer = tempLayer;
			holding.SetParent(null, true);
			holding = null;
		}
	}
}
