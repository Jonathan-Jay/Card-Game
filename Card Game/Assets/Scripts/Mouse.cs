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
	public int cardLayer;

	//has to be assigned outside of the class
	[System.Serializable]
	public class RaycastEvent : UnityEngine.Events.UnityEvent<RaycastHit> {
		public static RaycastEvent operator+(RaycastEvent rayEvent, UnityEngine.Events.UnityAction<RaycastHit> listener) {
			rayEvent.AddListener(listener);
			return rayEvent;
		}
		public static RaycastEvent operator-(RaycastEvent rayEvent, UnityEngine.Events.UnityAction<RaycastHit> listener) {
			rayEvent.RemoveListener(listener);
			return rayEvent;
		}
	}
	public event System.Action<RaycastHit> hoverEvent;
	public event System.Action<RaycastHit> clickEvent;
	//public RaycastEvent clickEvent;
	public event System.Action<RaycastHit> releaseEvent;
	//public RaycastEvent releaseEvent;

    private Camera cam;
	//used in the disabling of things
	int activeAnims = 0;
	int activeSpells = 0;
    // Start is called before the first frame update
    private void Awake() {
        cam = GetComponent<Camera>();
	}

	public Transform holding = null;

    // Update is called once per frame
    void Update() {
		if (disabled)	return;

        Ray rayInfo = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHitInfo;

        if (Physics.Raycast(rayInfo, out rayHitInfo, maxDist, ~(1 << ignoredLayer))) {
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
		clickEvent += ClickInteractable;
		releaseEvent += ReleaseCardHolder;
		clickEvent += ClickCard;
		releaseEvent += ReleaseCard;
	}

	public void DeActivateAll() {
		clickEvent -= ClickInteractable;
		releaseEvent -= ReleaseCardHolder;
		clickEvent -= ClickCard;
		releaseEvent -= ReleaseCard;
	}

	public void ActivateCard()
	{
		clickEvent += ClickCard;
		releaseEvent += ReleaseCard;
	}

	public void DeActivateCard()
	{
		clickEvent -= ClickCard;
		releaseEvent -= ReleaseCard;
	}

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

			clickEvent -= ClickInteractable;
	}

	public void DeactivateAnimationMode() {
		//if no more animations
		if (--activeAnims != 0)	return;

		clickEvent += ClickInteractable;
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
		if (cardTest != null && cardTest.player == player) {
			hit.rigidbody.isKinematic = true;
			cardTest.transform.SetParent(mouseObject, true);
			cardTest.transform.position += Vector3.up * vertOffset;
			cardTest.gameObject.layer = ignoredLayer;

			holding = cardTest.transform;
			return;
		}
	}
	void ClickInteractable(RaycastHit hit) {
		//first check if we wanna do smt
		GameObject hitObj = hit.transform.gameObject;
		if (!hitObj.CompareTag("Interactable"))	return;

		DeckManager deckTest = hitObj.GetComponent<DeckManager>();
		if (deckTest != null && deckTest.player == player) {
			Transform card = deckTest.DrawCard();
			if (card != null) {
				//render the card since it's the active player?
				card.GetComponent<Card>().RenderFace();

				card.GetComponent<Rigidbody>().isKinematic = true;
				card.SetParent(mouseObject, true);
				card.position += Vector3.up * vertOffset;
				card.gameObject.layer = ignoredLayer;

				holding = card;
			}
			return;
		}

		PressEventButton buttonTest = hitObj.GetComponent<PressEventButton>();
		if (buttonTest != null && (buttonTest.player == null || buttonTest.player == player)) {
			buttonTest.Press();
			return;
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
			return;
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
