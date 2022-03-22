using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base class
public class SpellEffect : MonoBehaviour
{
	public float duration = 0.5f;
	public float deathDelay = 0.25f;

	protected AudioQueue sounds;

	private void Awake() {
		sounds = GetComponent<AudioQueue>();
	}

	public void PerformEffect(SpellCard caster, PlayerData target, int index, int count) {
		StartCoroutine(DelayedCasting(((SpellData)caster.data).ability, target,
			index, caster, count));
	}

	public virtual float GetCardDeathDelay(int count) {
		return count * duration;
	}

	//default just moves the effect
	public virtual IEnumerator DelayedCasting(AbilityFunc ability, PlayerData target,
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
