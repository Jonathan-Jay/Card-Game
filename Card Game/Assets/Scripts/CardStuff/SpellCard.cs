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

	public override void RenderFace() {
		if (renderingFace || !data)	return;

		//dont do it cause render cost differently
		//base.RenderFace();

		//copy pasted from the above
		nameMesh.text = data.cardName;

		frontFace.material.mainTexture = data.cardArt;

		costMesh.text = (data.cost) + "*";

		//dirty flag
		renderingFace = true;

		descriptionMesh.text = ((SpellData)data).cardDescription;
	}

	public override void HideFace() {
		if (!renderingFace)	return;

		base.HideFace();

		descriptionMesh.text = "";
	}

	public override void OnPlace(PlayerData current, PlayerData opposing) {
		//don't render face until it's placed
		//RenderFace();
		StartCoroutine(CastSpell(current, opposing));
	}

	IEnumerator CastSpell(PlayerData current, PlayerData opposing) {
		RaycastHit hit = new RaycastHit();
		void UpdateRaycastHit(RaycastHit rayHit) {
			hit = rayHit;
		}

		//stop mouse from working immediately
		player.hand.input.ActivateSpellMode();
		player.hand.input.clickEvent += UpdateRaycastHit;

		yield return new WaitForSeconds(0.25f);

		Vector3 startPos = transform.localPosition;
		Vector3 endPos = Vector3.up * 0.25f;

		//do spell thing
		int newIndex = placement.index;
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

		//relink mouse functions
		player.hand.input.clickEvent -= UpdateRaycastHit;

		//can't target something, so just drop card
		if (newIndex < -1) {
			//relink mouse functions
			player.hand.input.DeactivateSpellMode();
			Release();
			//return the mana cost
			current.ReduceMana(-data.cost);
			yield break;
		}

		//now we can render the face
		base.OnPlace(null, null);
		
		//cast spell should take card of this
		((SpellData)data).CastSpell(this, target, newIndex);
	}

	public void SelfDestruct(float delay) {
		StartCoroutine(DelayedDeath(delay));
	}

	IEnumerator DelayedDeath(float delay) {
		yield return new WaitForSeconds(delay);
		
		player.hand.input.DeactivateSpellMode();
		StartCoroutine("Death");
	}
}
