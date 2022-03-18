using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAttacker : CardHolder
{
	public bool canDirectlyPut = false;
	public override int DoUpdate()
	{
		int dmg = 0;
		//all targetables are monsters for now
		if (holding != null && holding.targetable)
		{
			//assuming only direct attacks
			MonsterCard target = (MonsterCard)opposingData.field[index].holding;

			//can directly attack the opponent if target is empty
			dmg = ((MonsterCard)holding).Attack(target);

			//if target was killed
			if (target && !target.placement) {
				opposingData.backLine[index].DoUpdate();
			}
		}
		return dmg;
	}

	public override bool PutCard(Card card)
	{
		//either allowed to put or moving
		if (canDirectlyPut || card.placement) {
			return base.PutCard(card);
		}
		return false;
	}

	public override IEnumerator CardTransition(bool callPlace, bool disabledAnimationMode)
	{
		holding.GetComponent<Rigidbody>().isKinematic = true;
		holding.gameObject.layer = playerData.hand.input.ignoredLayer;

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
			if (callPlace)
				holding.OnPlace(playerData, opposingData);
			cardTrans.localPosition = floatingHeight;
		}

		//play the sound
		audioPlayer?.Play();
	}
}
