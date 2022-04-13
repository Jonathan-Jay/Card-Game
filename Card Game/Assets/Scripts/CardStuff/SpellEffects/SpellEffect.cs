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
		if (sounds)
			deathDelay = sounds.GetAudioSource(0).clip.length;
	}

	public void PerformEffect(SpellCard caster, PlayerData target, int index, int count) {
		StartCoroutine(DelayedCasting(((SpellData)caster.data).ability, target,
			index, caster, count));
	}

	public virtual float GetCardDeathDelay(int count) {
		return count * duration;
	}

	//default just does nothing
	public virtual IEnumerator DelayedCasting(AbilityFunc ability, PlayerData target,
		int index, SpellCard caster, int count)
	{
		yield return new WaitForSeconds(count * duration);

		//instant is default
		ability?.Invoke(target, index, (SpellData)caster.data);
		sounds.Play();

		yield return new WaitForSeconds(deathDelay);

		Destroy(gameObject);
	}
}
