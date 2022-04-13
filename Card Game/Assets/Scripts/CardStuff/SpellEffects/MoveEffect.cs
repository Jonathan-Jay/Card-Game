using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base class
public class MoveEffect : SpellEffect
{
	public Vector3 offset = Vector3.up * 0.5f;
	public override float GetCardDeathDelay(int count) {
		return count * duration;
	}

	public override IEnumerator DelayedCasting(AbilityFunc ability, PlayerData target,
		int index, SpellCard caster, int count)
	{
		transform.position = caster.transform.position + caster.transform.rotation * offset;

		Quaternion targetRot = target.hand.transform.rotation;

		Vector3 targetPos = target.hand.transform.position + targetRot * offset;
		if (index >= 0) {
			if (index >= target.field.Count && target.backLine[index - target.field.Count].holding) {
				targetRot = target.backLine[index - target.field.Count].transform.rotation;
				targetPos = target.backLine[index - target.field.Count].transform.position + targetRot * offset;
			}
			else if (target.field[index].holding) {
				targetRot = target.field[index].transform.rotation;
				targetPos = target.field[index].transform.position + targetRot * offset;
			}
		}
		yield return new WaitForSeconds(count * duration);

		float oneOverDelay = 1f/duration;
		float val;

		for (float i = 0; i < duration; i += Time.deltaTime) {
			val = Mathf.SmoothStep(0f, 1f, oneOverDelay * i);
			transform.position = Vector3.Lerp(transform.position, targetPos, val);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, val);
			
			yield return Card.eof;
		}

		ability?.Invoke(target, index, (SpellData)caster.data);
		sounds.Play();

		//kill the thing
		oneOverDelay = 1f/deathDelay;
		for (float i = deathDelay; i > 0; i -= Time.deltaTime) {
			transform.localScale = Vector3.one * oneOverDelay * i;
			yield return Card.eof;
		}
		Destroy(gameObject);
	}
}
