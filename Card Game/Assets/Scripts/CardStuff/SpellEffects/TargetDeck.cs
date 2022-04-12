using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base class
public class TargetDeck : SpellEffect
{
	public override float GetCardDeathDelay(int count) {
		return duration * count;
	}

	//default just moves the effect
	public override IEnumerator DelayedCasting(AbilityFunc ability, PlayerData target,
		int index, SpellCard caster, int count)
	{
		transform.position = caster.transform.position + Vector3.up * 0.5f;

		Vector3 targetPos = target.deck.transform.position + Vector3.up * 0.5f;
		
		yield return new WaitForSeconds(count * duration);

		float oneOverDelay = 1f/duration;
		for (float i = 0; i < duration; i += Time.deltaTime) {
			transform.position = Vector3.Lerp(transform.position, targetPos,
				Mathf.SmoothStep(0f, 1f, oneOverDelay * i));
			yield return Card.eof;
		}

		ability?.Invoke(target, index, (SpellData)caster.data);
		sounds.Play();

		//kill the thing
		oneOverDelay = 1f/deathDelay;
		for (float i = deathDelay; i > 0; i -= Time.deltaTime) {
			transform.localScale = Vector3.one * oneOverDelay * i;
		}
		Destroy(gameObject);
	}
}
