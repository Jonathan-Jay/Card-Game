using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHolder : MonoBehaviour
{
	public Card holding;
	[SerializeField] Color hasCard = Color.white;
	Color originalCol;
	public PlayerData playerData;
	public PlayerData opposingData;
	public int index = -1;
	public Vector3 floatingHeight = Vector3.up * 0.1f;
	public float moveSpeed = 1f;
	public float rotSpeed = 1f;
	public Vector3 slamHeight = Vector3.up * 0.1f;
	public float slamSpeed = 10f;
	public string interactableTag= "Interactable";
	public int defaultCardLayer = 6;

	[SerializeField] protected ParticleSystem slamParticles;
	protected AudioQueue audioPlayer;
	private void Awake() {
		audioPlayer = GetComponent<AudioQueue>();
	}

	void Start() {
		originalCol = GetComponentInChildren<MeshRenderer>().material.color;
	}

	//normal holders only update boosts
    public virtual void DoUpdate() {
		if (holding && holding.targetable) {
			((MonsterCard)holding).UpdateBoosts();
		}
	}

	//returns true on sucess
	public virtual bool PutCard(Card card) {
		//check if holding smt already, if valid player, and cost
		if (holding != null || card.player != playerData) {	return false;	}

		//if moved
		bool newCard = true;
		if (card.placement != null) {
			card.Release(false);
			newCard = false;
		}
		//else it's a newly placed card, so check cost
		else if (!card.data.CheckCost(playerData)) {
			return false;
		}

		//also allow other player to see the card, send a message to the server to set data
		card.transform.SetParent(transform, true);
		card.placement = this;
		card.placed = true;
		card.tag = playerData.playerTag;
		holding = card;
		GetComponentInChildren<MeshRenderer>().material.color = hasCard;

		card.PrePlace(playerData, opposingData);

		StartCoroutine(CardTransition(newCard, newCard && (card.data.cost > 0)));

		return true;
	}

	//remove holding
	public void UnLink() {
		holding.gameObject.layer = defaultCardLayer;
		holding.gameObject.tag = interactableTag;
		holding = null;
		//material change here
		GetComponentInChildren<MeshRenderer>().material.color = originalCol;
	}

	public virtual IEnumerator CardTransition(bool callPlace, bool disabledAnimationMode) {
		//so that you cant double place sacrifice cards
		if (disabledAnimationMode) {
			//we need to unlink it
			playerData.hand.input.holding = null;
			if (ServerManager.CheckIfClient(playerData, true)) {
				playerData.hand.input.ActivateSpellMode();
			}
		}
		else if (ServerManager.CheckIfClient(playerData, true)) {
			playerData.hand.input.ActivateAnimationMode();
		}

		Transform cardTrans = holding.transform;
		while (holding != null && !holding.moving) {
			cardTrans.localPosition = Vector3.Lerp(cardTrans.localPosition, slamHeight,
				moveSpeed * Time.deltaTime);
			cardTrans.localRotation = Quaternion.Slerp(cardTrans.localRotation, Quaternion.identity,
				rotSpeed * Time.deltaTime);
			if (Quaternion.Angle(cardTrans.localRotation, Quaternion.identity) < 1f &&
				Vector3.Distance(cardTrans.localPosition, slamHeight) < 0.01f) {
					break;
				}
			yield return Card.eof;
		}
		while (holding != null && !holding.moving && cardTrans.localPosition != floatingHeight) {
			cardTrans.localPosition = Vector3.MoveTowards(
					cardTrans.localPosition, floatingHeight, slamSpeed * Time.deltaTime);
			yield return Card.eof;
		}
		// > this thing broke the network code my god i hate this?
		//yield return new WaitForSeconds(0.25f);

		if (disabledAnimationMode) {
			//we need to unlink it
			if (ServerManager.CheckIfClient(playerData, true)) {
				playerData.hand.input.DeactivateSpellMode(true);
			}
		}
		else if (ServerManager.CheckIfClient(playerData, true)) {
			playerData.hand.input.DeactivateAnimationMode();
		}

		//final fix in case
		if (holding != null) {
			//now valid
			if (callPlace)
				holding.OnPlace(playerData, opposingData);
			
			slamParticles.Play();
			//play the sound
			audioPlayer?.Play();
		}
	}
}
