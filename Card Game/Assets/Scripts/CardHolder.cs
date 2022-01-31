using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHolder : MonoBehaviour
{
	Card holding;
	public int index;
	public Vector3 floatingHeight = Vector3.up * 0.1f;

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

		card.transform.SetParent(transform, false);
		card.transform.position = floatingHeight;

		//should be flipped, but animate it?
		card.transform.rotation = Quaternion.identity;
		holding = card;
		return true;
	}
}
