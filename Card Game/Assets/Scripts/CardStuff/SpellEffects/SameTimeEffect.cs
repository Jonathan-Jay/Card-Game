using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameTimeEffect : SpellEffect
{
	public override float GetCardDeathDelay(int count) {
		return duration;
	}

	public override IEnumerator DelayedCasting(AbilityFunc ability, PlayerData target,
		int index, SpellCard caster, int count)
	{
		transform.position = caster.transform.position + Vector3.up * 0.5f;

		Vector3 targetPos = target.hand.transform.position + Vector3.up;
		if (index >= 0) {
			if (index >= target.field.Count && target.backLine[index - target.field.Count].holding)
				targetPos = target.backLine[index - target.field.Count].transform.position + Vector3.up * 0.5f;
			else if (target.field[index].holding)
				targetPos = target.field[index].transform.position + Vector3.up * 0.5f;
		}
		//no delay delay
		//yield return new WaitForSeconds(count * duration);

		float oneOverDelay = 1f/duration;
		for (float i = 0; i < duration; i += Time.deltaTime) {
			transform.position = Vector3.Lerp(transform.position, targetPos,
				Mathf.SmoothStep(0f, 1f, oneOverDelay * i));
			yield return Card.eof;
		}

		ability?.Invoke(target, index, (SpellData)caster.data);
		if (count == 0)
			sounds.Play();

		//kill the thing
		oneOverDelay = 1f/deathDelay;
		for (float i = deathDelay; i > 0; i -= Time.deltaTime) {
			transform.localScale = Vector3.one * oneOverDelay * i;
		}
		Destroy(gameObject);
	}
}
