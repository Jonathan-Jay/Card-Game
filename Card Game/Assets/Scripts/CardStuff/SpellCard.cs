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

	public override void OnPlace(int index, GameController.PlayerData current,
		GameController.PlayerData opposing)
	{
		StartCoroutine(CastSpell(current, opposing, index));
	}

	IEnumerator CastSpell(GameController.PlayerData current,
		GameController.PlayerData opposing, int index)
	{
		yield return new WaitForSeconds(0.25f);
		//do spell thing
		//GameController.PlayerData target = null;
		//int newIndex = index;
		int newIndex = -1;
		Card target = null;
		while (target == null) {
			yield return eof;
			target = ((SpellData)data).targetting.Invoke(this, new RaycastHit());
		}

		//can't target something, so just drop card
		if (newIndex < 0) {
			if (placement != null)
				placement.UnLink();
			//return the mana cost
			current.currentMana += data.cost;
			yield break;
		}

		((SpellData)data).CastSpell(opposing, newIndex);

		//then die
		StartCoroutine("Death");
	}
}
