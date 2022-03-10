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
	public int index;
	public Vector3 floatingHeight = Vector3.up * 0.1f;
	public float moveSpeed = 1f;
	public float rotSpeed = 1f;
	public Vector3 slamHeight = Vector3.up * 0.1f;
	public float slamSpeed = 10f;
	public string interactableTag= "Interactable";
	public int defaultCardLayer;

	void Start() {
		originalCol = GetComponentInChildren<MeshRenderer>().material.color;
	}

	//damage to player
    public int DoUpdate()
    {
        if (holding == null)	return 0;

		//assuming only direct attacks
		MonsterCard target = (MonsterCard)opposingData.field[index].holding;
		//hit player if not facing anything
		if (target == null) {
			return ((MonsterCard)holding).currAttack;
		}
		//assuming no overkill system (Attack returns overkill)
		((MonsterCard)holding).Attack(target);
		return 0;
    }

	//returns true on sucess
	public bool PutCard(Card card)
	{
		//check if holding smt already, if valid player, and cost
		if (holding != null || card.player != playerData || !card.data.CheckCost(playerData)) {	return false;	}

		//also allow other player to see the card, send a message to the server to set data

		card.transform.SetParent(transform, true);
		card.placement = this;
		card.tag = playerData.playerTag;
		holding = card;
		GetComponentInChildren<MeshRenderer>().material.color = hasCard;
		StartCoroutine("CardTransition");

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

	IEnumerator CardTransition() {
		Transform cardTrans = holding.transform;
		while (holding != null) {
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
		while (holding != null && cardTrans.localPosition != floatingHeight) {
			cardTrans.localPosition = Vector3.MoveTowards(
					cardTrans.localPosition, floatingHeight, slamSpeed * Time.deltaTime);
			yield return Card.eof;
		}
		//final fix in case
		if (holding != null) {
			//now valid
			holding.OnPlace(index, playerData, opposingData);
			cardTrans.localPosition = floatingHeight;
		}
	}
}
