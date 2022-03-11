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
			if (renderingFace) {
				//because of the dirty flag
				renderingFace = false;
				RenderFace();
			}
		}
	}

	public override void SetData(CardData newData) {
		base.SetData(newData);
	}

	public override void RenderFace()
	{
		if (renderingFace || !data)	return;

		base.RenderFace();

		descriptionMesh.text = ((SpellData)data).cardDescription;
	}

	public override void OnPlace(int index, PlayerData current, PlayerData opposing) {
		RenderFace();
		StartCoroutine(CastSpell(current, opposing, index));
	}

	IEnumerator CastSpell(PlayerData current, PlayerData opposing, int index) {
		RaycastHit hit = new RaycastHit();
		void UpdateRaycastHit(RaycastHit rayHit) {
			hit = rayHit;
		}

		//stop mouse from working immidiately
		player.hand.input.ActivateSpellMode();
		player.hand.input.clickEvent += UpdateRaycastHit;

		yield return new WaitForSeconds(0.25f);

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
		player.hand.input.clickEvent += UpdateRaycastHit;
		player.hand.input.DeactivateSpellMode();

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
