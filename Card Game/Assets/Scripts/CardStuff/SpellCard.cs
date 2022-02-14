using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellCard : Card
{
	[SerializeField] protected TMP_Text descriptionMesh;
	WaitForEndOfFrame frameWait = new WaitForEndOfFrame();

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public override void SetData(CardData newData) {
		base.SetData(newData);
	}

	public override void OnPlace()
	{
		StartCoroutine("CastSpell");
	}

	IEnumerator CastSpell() {
		yield return new WaitForSeconds(0.25f);
		//do spell thing
		Card target = null;
		while (target == null) {
			yield return frameWait;
			target = ((SpellData)data).targetting.Invoke(this, new RaycastHit());
		}

		//to check if targetting self or opponent (maybe limit this)
		if (target == this)	target = null;
		((SpellData)data).CastSpell(target);

		//then die
		StartCoroutine("Death");
	}
}
