using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base class
public class MoveCardEffect : SpellEffect
{
	public Vector3 cardOffset = Vector3.up * 0.1f + Vector3.forward * 0.75f;
	public Quaternion cardRot = Quaternion.Euler(0f, 180f, 0f);
	public override float GetCardDeathDelay(int count) {
		return count * duration;
	}

	public override IEnumerator DelayedCasting(AbilityFunc ability, PlayerData target,
		int index, SpellCard caster, int count)
	{
		if (index >= 0) {
			if (index >= target.field.Count && target.backLine[index - target.field.Count].holding) {
				cardRot = target.backLine[index - target.field.Count].transform.rotation * cardRot;
				cardOffset = target.backLine[index - target.field.Count].transform.position +
					 cardRot * cardOffset;
			}
			else if (target.field[index].holding) {
				cardRot = target.field[index].transform.rotation * cardRot;
				cardOffset = target.field[index].transform.position +
					 cardRot * cardOffset;
			}
		}
		else {
			cardRot = target.hand.transform.rotation * cardRot;
			cardOffset = target.hand.transform.position + Vector3.up;
		}
		yield return new WaitForSeconds(count * duration);

		float oneOverDelay = 1f/duration;
		float val;

		for (float i = 0; i < duration; i += Time.deltaTime) {
			val = Mathf.SmoothStep(0f, 1f, oneOverDelay * i);
			caster.transform.position = Vector3.Lerp(caster.transform.position, cardOffset, val);
			caster.transform.rotation = Quaternion.Lerp(caster.transform.rotation, cardRot, val);

			yield return Card.eof;
		}

		ability?.Invoke(target, index, (SpellData)caster.data);
		sounds.Play();

		//kill the thing instantly
		Destroy(gameObject);
	}
}
