using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHolder : MonoBehaviour
{
	Card holding;
	[SerializeField] Color hasCard = Color.white;
	Color originalCol;
	public string playerTag;
	public int index;
	public Vector3 floatingHeight = Vector3.up * 0.1f;
	public float moveSpeed = 1f;
	public float rotSpeed = 1f;
	public Vector3 slamHeight = Vector3.up * 0.1f;
	public float slamSpeed = 10f;

	void Start() {
		originalCol = GetComponentInChildren<MeshRenderer>().material.color;
	}

	//damage to player
    public int DoUpdate(List<CardHolder> opposing)
    {
        if (holding == null)	return 0;

		//assuming only direct attacks
		MonsterCard target = (MonsterCard)opposing[index].holding;
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
		if (holding != null)	return false;

		card.transform.SetParent(transform, true);
		card.placement = this;
		card.tag = playerTag;
		holding = card;
		GetComponentInChildren<MeshRenderer>().material.color = hasCard;
		StartCoroutine("CardTransition");

		return true;
	}

	//remove holding
	public void UnLink() {
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
					//now valid
					holding.OnPlace();
					break;
				}
			yield return new WaitForEndOfFrame();
		}
		for (float i = 0; i < 1 && holding != null; i += slamSpeed * Time.deltaTime) {
			cardTrans.localPosition = Vector3.Lerp(cardTrans.localPosition, floatingHeight, i);
			yield return new WaitForEndOfFrame();
		}
		//final fix in case
		if (holding != null) {
			cardTrans.localPosition = floatingHeight;
		}
	}
}
