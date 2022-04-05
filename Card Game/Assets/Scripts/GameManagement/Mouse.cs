using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour {
    public Transform mouseObject;
    public MeshRenderer defaultCursor;
    public MeshRenderer targettingCursor;
	public Color defaultCol = Color.green;
	public Color hoverCol = Color.red;
	public PlayerData player;
	public bool disabled = false;
    public float maxDist = 0f;
    public float vertOffset = 0f;
	public int ignoredLayer;
	public int invisibleLayer;
	public int cardLayer;
	
	public event System.Action<Transform> hoverEvent;
	public event System.Action<Transform> clickEvent;
	//public RaycastEvent clickEvent;
	public event System.Action<Transform> releaseEvent;
	//public RaycastEvent releaseEvent;

    private Camera cam;
	int mask;


	//used in the disabling of things
	public int activeAnims {get; private set;} = 0;
	public int activeSpells { get; private set; } = 0;
    // Start is called before the first frame update
    private void Awake() {
        cam = GetComponent<Camera>();
		//just do this once
		mask = ~((1 << ignoredLayer) | (1 << invisibleLayer));
		targettingCursor.gameObject.SetActive(false);
	}

	private void OnEnable() {
		hoverEvent += ColourChange;
	}

	private void OnDisable() {
		hoverEvent -= ColourChange;
	}

	bool onValidCard = false;
	void ColourChange(Transform hit) {
		if (!hit)	return;

		//if in spell mode
		if (activeSpells > 0) {
			CardHolder test = hit.GetComponent<CardHolder>();
			//if over a monster, it's red
			if (test && test.holding && test.holding.targetable) {
				if (!onValidCard) {
					targettingCursor.material.color = hoverCol;
					onValidCard = true;
				}
			}
			else if (onValidCard) {
				targettingCursor.material.color = defaultCol;
				onValidCard = false;
			}
		}
		else {
			//what do we do when not targetting? anythign interactable
			if (hit.CompareTag("Interactable")) {
				if (!onValidCard) {
					defaultCursor.material.color = hoverCol;
					onValidCard = true;
				}
			}
			else if (onValidCard) {
				defaultCursor.material.color = defaultCol;
				onValidCard = false;
			}
		}
	}

	public Transform holding = null;

	public void Drop() {
		if (holding) {
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.gameObject.layer = cardLayer;
			holding.SetParent(null, true);
			holding = null;
		}
	}

	//a seperate value to ignore
	public void SetDisabled(bool val) {
		//drop anything we may be holding
		Drop();

		disabled = val;
	}

	bool ignore = false;
	public void IgnoreInput(bool val) {
		ignore = val;
	}

	public void ForwardHoverEvent(Transform hit) {
		hoverEvent?.Invoke(hit);
	}

	public void ForwardClickEvent(Transform hit) {
		clickEvent?.Invoke(hit);
	}

	public void ForwardReleaseEvent(Transform hit) {
		releaseEvent?.Invoke(hit);
	}

    // Update is called once per frame
    void Update() {
		if (disabled || ignore)	return;

        Ray rayInfo = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHitInfo;

        if (Physics.Raycast(rayInfo, out rayHitInfo, maxDist, mask)) {
            mouseObject.position = rayHitInfo.point + Vector3.up * vertOffset;
			
			hoverEvent?.Invoke(rayHitInfo.transform);

			if (Input.GetMouseButtonDown(0)) {
				clickEvent?.Invoke(rayHitInfo.transform);
			}
        }
		
		if (Input.GetMouseButtonUp(0)) {
			//can be null
			releaseEvent?.Invoke(rayHitInfo.transform);
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
		Drop();

		clickEvent += ClickCard;
		releaseEvent += ReleaseCardHolder;
		releaseEvent += ReleaseCard;
	}

	public void UnLinkInteractablesFunc() {
		//make sure we're not holding anything
		Drop();

		clickEvent -= ClickCard;
		releaseEvent -= ReleaseCardHolder;
		releaseEvent -= ReleaseCard;
	}


	//for networking
	public static byte[] animationModeOn = System.Text.Encoding.ASCII.GetBytes("INPANMON");
	public static byte[] animationModeOff = System.Text.Encoding.ASCII.GetBytes("INPANMOF");
	public static byte[] spellModeOn = System.Text.Encoding.ASCII.GetBytes("INPSPLON");
	public static byte[] spellModeOff = System.Text.Encoding.ASCII.GetBytes("INPSPLOF");


	//only allows for holding and placing cards
	public void ActivateAnimationMode(bool trySend = true) {
		//if more animations
		if (activeAnims++ != 0)	return;

		clickEvent -= ClickButton;
		//clickEvent -= ClickDeck;

		if (trySend && !ServerManager.localMultiplayer)
			Client.SendGameData(animationModeOn);
	}

	public void DeactivateAnimationMode(bool trySend = true) {
		//if no more animations
		if (--activeAnims != 0)	return;

		clickEvent += ClickButton;
		//clickEvent += ClickDeck;

		if (trySend && !ServerManager.localMultiplayer)
			Client.SendGameData(animationModeOff);
	}

	//to turn off animation mode (avoid weird desync issues in networking, vm, seems to still do issues lol)
	//public bool disabledAnimationMode = false;

	public void ActivateSpellMode(bool trySend = true) {
		if (activeSpells++ != 0)	return;

		//if (disabledAnimationMode)
		UnLinkInteractablesFunc();
		ActivateAnimationMode(false);

		if (trySend && !ServerManager.localMultiplayer)
			Client.SendGameData(spellModeOn);
		
		//change rendering
		targettingCursor.gameObject.SetActive(true);
		defaultCursor.gameObject.SetActive(false);
		defaultCursor.material.color = defaultCol;
	}

	public void DeactivateSpellMode(bool isPlayer, bool trySend = true) {
		if (--activeSpells != 0)	return;

		//if (disabledAnimationMode)
		LinkInteractablesFunc();
		DeactivateAnimationMode(isPlayer || false);

		if (trySend && !ServerManager.localMultiplayer)
			Client.SendGameData(spellModeOff);

		//change rendering
		targettingCursor.gameObject.SetActive(false);
		targettingCursor.material.color = defaultCol;
		defaultCursor.gameObject.SetActive(true);
	}

	void ClickCard(Transform hit) {
		//first check if we wanna do smt
		if (!hit || !hit.CompareTag("Interactable"))	return;

		Card cardTest = hit.GetComponent<Card>();
		if (cardTest == null || cardTest.player != player) return;

		hit.GetComponent<Rigidbody>().isKinematic = true;
		cardTest.transform.SetParent(mouseObject, true);
		Vector3 pos = cardTest.transform.localPosition;
		pos.y = vertOffset;
		cardTest.transform.localPosition = pos;
		cardTest.gameObject.layer = ignoredLayer;

		holding = cardTest.transform;
	}

	/*
	void ClickDeck(Transform hit) {
		//first check if we wanna do smt
		GameObject hitObj = hit.gameObject;
		if (!hitObj.CompareTag("Interactable"))	return;

		DeckManager deckTest = hitObj.GetComponent<DeckManager>();
		if (deckTest != null && deckTest.player == player) {
			deckTest.FirstDraw(true);
		}
	}*/

	void ClickButton(Transform hit) {
		if (!hit || !hit.CompareTag("Interactable")) return;

		PressEventButton buttonTest = hit.GetComponent<PressEventButton>();
		if (buttonTest != null && buttonTest.enabled && (buttonTest.player == null || buttonTest.player == player)) {
			buttonTest.Press();
		}
	}

	void ReleaseCardHolder(Transform hit) {
		//all in one lol
		if (!holding || !hit || !hit.CompareTag("Interactable"))	return;

		CardHolder tempHolder = hit.GetComponent<CardHolder>();
		if (tempHolder != null && tempHolder.PutCard(holding.GetComponent<Card>())) {
			//dont allow cards to be interactable when in holder (aka dont change the layer)
			//holding.gameObject.layer = tempLayer;
			holding = null;
		}
	}

	void ReleaseCard(Transform hit) {
		if (!holding) return;

		//just drop the card
		holding.gameObject.layer = cardLayer;

		//check if colliding with hand maybe?
		if (hit && hit.parent == player.hand.transform) {
			//put in hand
			holding.GetComponent<Card>()?.CallBackCard();
		}
		else {
			holding.GetComponent<Rigidbody>().isKinematic = false;
			holding.SetParent(null, true);
		}
		
		holding = null;
	}
}
