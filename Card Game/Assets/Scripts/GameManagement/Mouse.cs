using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
	public PlayerData player;
	public bool disabled = false;
    public float maxDist = 0f;
    public float vertOffset = 0f;
	public int ignoredLayer;
	public int invisibleLayer;
	public int cardLayer;
	
	public event System.Action<RaycastHit> hoverEvent;
	public event System.Action<RaycastHit> clickEvent;
	//public RaycastEvent clickEvent;
	public event System.Action<RaycastHit> releaseEvent;
	//public RaycastEvent releaseEvent;

    private Camera cam;
	int mask;

	//used in the disabling of things
	int activeAnims = 0;
	int activeSpells = 0;
    // Start is called before the first frame update
    private void Awake() {
        cam = GetComponent<Camera>();
		//just do this once
		mask = ~((1 << ignoredLayer) | (1 << invisibleLayer));
	}

	public Transform holding = null;

	bool ignore = false;
	public void IgnoreInput(bool val) {
		ignore = val;
	}

	public void DelayedEnableInput(float delay) {
		StartCoroutine(DelayedIgnoreSetter(true, delay));
	}

	public void DelayedIgnoreInput(bool val, float delay = 0.25f) {
		StartCoroutine(DelayedIgnoreSetter(val, delay));
	}

	IEnumerator DelayedIgnoreSetter(bool val, float delay) {
		yield return new WaitForSeconds(delay);
		ignore = val;
	}

    // Update is called once per frame
    void Update() {
		if (disabled || ignore)	return;

        Ray rayInfo = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHitInfo;

        if (Physics.Raycast(rayInfo, out rayHitInfo, maxDist, mask)) {
            mouseObject.position = rayHitInfo.point + Vector3.up * vertOffset;
			
			hoverEvent?.Invoke(rayHitInfo);

			if (Input.GetMouseButtonDown(0)) {
				clickEvent?.Invoke(rayHitInfo);
			}
			if (Input.GetMouseButtonUp(0)) {
				releaseEvent?.Invoke(rayHitInfo);
			}
        }
    }

	public void ActivateAll() {
		clickEvent += ClickButton;
		//clickEvent += ClickDeck;
		releaseEvent += ReleaseCardHolder;
		clickEvent += ClickCard;
		releaseEvent += ReleaseCard;
	}

	public void DeActivateAll() {
		clickEvent -= ClickButton;
		//clickEvent -= ClickDeck;
		releaseEvent -= ReleaseCardHolder;
		clickEvent -= ClickCard;
		releaseEvent -= ReleaseCard;
	}

	public void ActivateEssentials() {
		clickEvent += ClickButton;
		clickEvent += ClickCard;
		releaseEvent += ReleaseCard;
	}

	public void DeActivateEssentials() {
		clickEvent -= ClickButton;
		clickEvent -= ClickCard;
		releaseEvent -= ReleaseCard;
	}

	/*
	public void ActivateDeck() {
		clickEvent += ClickDeck;
	}

	public void DeActivateDeck() {
		clickEvent -= ClickDeck;
	}*/

	public void LinkInteractablesFunc() {
		//make sure we're not holding anything
		if (holding) {
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.gameObject.layer = cardLayer;
			holding.SetParent(null, true);
			holding = null;
		}
		clickEvent += ClickCard;
		releaseEvent += ReleaseCardHolder;
		releaseEvent += ReleaseCard;
	}

	public void UnLinkInteractablesFunc() {
		//make sure we're not holding anything
		if (holding) {
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.gameObject.layer = cardLayer;
			holding.SetParent(null, true);
			holding = null;
		}
		clickEvent -= ClickCard;
		releaseEvent -= ReleaseCardHolder;
		releaseEvent -= ReleaseCard;
	}

	//only allows for holding and placing cards
	public void ActivateAnimationMode() {
		//if more animations
		if (activeAnims++ != 0)	return;

		clickEvent -= ClickButton;
		//clickEvent -= ClickDeck;
	}

	public void DeactivateAnimationMode() {
		//if no more animations
		if (--activeAnims != 0)	return;

		clickEvent += ClickButton;
		//clickEvent += ClickDeck;
	}

	public void ActivateSpellMode() {
		if (activeSpells++ != 0)	return;

		UnLinkInteractablesFunc();
		ActivateAnimationMode();
		//change rendering
		
	}

	public void DeactivateSpellMode() {
		if (--activeSpells != 0)	return;

		LinkInteractablesFunc();
		DeactivateAnimationMode();
		//change rendering

	}

	void ClickCard(RaycastHit hit) {
		//first check if we wanna do smt
		GameObject hitObj = hit.transform.gameObject;
		if (!hitObj.CompareTag("Interactable"))	return;

		Card cardTest = hitObj.GetComponent<Card>();
		if (cardTest == null || cardTest.player != player) return;

		hit.rigidbody.isKinematic = true;
		cardTest.transform.SetParent(mouseObject, true);
		cardTest.transform.localPosition = Vector3.up * vertOffset;
		cardTest.gameObject.layer = ignoredLayer;

		holding = cardTest.transform;
	}

	/*
	void ClickDeck(RaycastHit hit) {
		//first check if we wanna do smt
		GameObject hitObj = hit.transform.gameObject;
		if (!hitObj.CompareTag("Interactable"))	return;

		DeckManager deckTest = hitObj.GetComponent<DeckManager>();
		if (deckTest != null && deckTest.player == player) {
			deckTest.FirstDraw(true);
		}
	}*/

	void ClickButton(RaycastHit hit) {
		GameObject hitObj = hit.transform.gameObject;
		if (!hitObj.CompareTag("Interactable")) return;

		PressEventButton buttonTest = hitObj.GetComponent<PressEventButton>();
		if (buttonTest != null && buttonTest.enabled && (buttonTest.player == null || buttonTest.player == player)) {
			buttonTest.Press();
		}
	}

	void ReleaseCardHolder(RaycastHit hit) {
		if (!holding) return;

		GameObject hitObj = hit.transform.gameObject;
		//cardholder test
		if (!hitObj.CompareTag("Interactable"))	return;

		CardHolder tempHolder = hitObj.GetComponent<CardHolder>();
		if (tempHolder != null && tempHolder.PutCard(holding.GetComponent<Card>())) {
			//dont allow cards to be interactable when in holder (aka dont change the layer)
			//holding.gameObject.layer = tempLayer;
			holding = null;
		}
	}

	void ReleaseCard(RaycastHit hit) {
		if (!holding) return;

		//just drop the card otherwise
		holding.GetComponent<Rigidbody>().isKinematic = false;
		holding.gameObject.layer = cardLayer;
		holding.SetParent(null, true);
		holding = null;
	}
}
