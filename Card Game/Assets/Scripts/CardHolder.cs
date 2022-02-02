using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHolder : MonoBehaviour
{
	Card holding;
	public int index;
	public Vector3 floatingHeight = Vector3.up * 0.1f;
	public float moveSpeed = 1f;
	public float rotSpeed = 1f;

	//damage to player
    public int DoUpdate(List<CardHolder> opposing)
    {
        if (holding == null)	return 0;

		//assuming only direct attacks
		Card target = opposing[index].holding;
		//hit player if not facing anything
		if (target == null) {
			return holding.currAttack;
		}
		//assuming no overkill system (Attack returns overkill)
		holding.Attack(target);
		return 0;
    }

	//returns true on sucess
	public bool PutCard(Card card)
	{
		if (holding != null)	return false;

		card.transform.SetParent(transform, true);
		holding = card;
		StartCoroutine("CardTransition");

		return true;
	}

	IEnumerator CardTransition() {
		Transform cardTrans = holding.transform;
		while (holding != null) {
			cardTrans.localPosition = Vector3.Lerp(cardTrans.localPosition, floatingHeight,
				moveSpeed * Time.deltaTime);
			cardTrans.localRotation = Quaternion.Slerp(cardTrans.localRotation, Quaternion.identity,
				rotSpeed * Time.deltaTime);
			if (Quaternion.Angle(cardTrans.localRotation, Quaternion.identity) < 0.1f &&
				Vector3.Distance(cardTrans.localPosition, floatingHeight) < 0.01f) {
					//now valid
					break;
				}
			yield return new WaitForEndOfFrame();
		}
	}
}
