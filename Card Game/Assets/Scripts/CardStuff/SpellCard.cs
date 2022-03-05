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

		RaycastHit hit = new RaycastHit();

		void UpdateRaycastHit(RaycastHit rayHit) {
			hit = rayHit;
		}

		//stop mouse from working
		hand.input.ActivateSpellMode();
		hand.input.clickEvent += UpdateRaycastHit;

		Vector3 startPos = transform.localPosition;
		Vector3 endPos = Vector3.up * 0.25f;

		//do spell thing
		int newIndex = index;
		PlayerData target = null;
		while (target == null) {
			yield return eof;
			target = ((SpellData)data).targetting.Invoke(current, opposing, ref newIndex, ref hit);
			//levitate card
			transform.localPosition = Vector3.MoveTowards(transform.localPosition,
				endPos, 2f * Time.deltaTime);
		}

		while (transform.localPosition != startPos) {
			yield return eof;
			transform.localPosition = Vector3.MoveTowards(transform.localPosition,
				startPos, 4f * Time.deltaTime);
		}

		//relinnk mouse functions
		hand.input.clickEvent += UpdateRaycastHit;
		hand.input.DeactivateSpellMode();

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
