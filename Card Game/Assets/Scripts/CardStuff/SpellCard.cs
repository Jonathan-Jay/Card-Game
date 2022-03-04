using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellCard : Card
{
	[SerializeField] protected TMP_Text descriptionMesh;

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public override void SetData(CardData newData) {
		base.SetData(newData);
		descriptionMesh.text = ((SpellData)newData).cardDescription;
	}

	public override void OnPlace(int index, PlayerData current, PlayerData opposing) {
		StartCoroutine(CastSpell(current, opposing, index));
	}

	IEnumerator CastSpell(PlayerData current, PlayerData opposing, int index) {
		yield return new WaitForSeconds(0.25f);

		//do spell thing
		int newIndex = index;
		PlayerData target = null;
		while (target == null) {
			yield return eof;
			target = ((SpellData)data).targetting.Invoke(current, opposing, ref newIndex, new RaycastHit());
		}

		//can't target something, so just drop card
		if (newIndex < -1) {
			Release();
			//return the mana cost
			current.ReduceMana(-data.cost);
			yield break;
		}

		((SpellData)data).CastSpell(target, newIndex);

		//then die
		StartCoroutine("Death");
	}
}
