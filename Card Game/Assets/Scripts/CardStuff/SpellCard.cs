using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellCard : Card
{
	[SerializeField] protected TMP_Text descriptionMesh;

	[SerializeField] AudioQueue placementSound;

	private void Awake() {
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

		flavourTextMesh.gameObject.SetActive(data.flavourText != "");
		if (flavourTextMesh.gameObject.activeInHierarchy)
			flavourTextMesh.text = data.flavourText;

		//dirty flag
		renderingFace = true;

		descriptionMesh.text = ((SpellData)data).cardDescription;
	}

	public override void HideFace() {
		if (!renderingFace)	return;

		base.HideFace();

		descriptionMesh.text = "";
	}

	public override void PrePlace(PlayerData current, PlayerData opposing) {
		placementSound?.Play();

		StartCoroutine(CastSpell(current, opposing));
	}

	public override void OnPlace(PlayerData current, PlayerData opposing) {
		//don't render face until it's placed
		//RenderFace();

		//StartCoroutine(CastSpell(current, opposing));
		waiting = true;
	}

	bool waiting = false;
	IEnumerator CastSpell(PlayerData current, PlayerData opposing) {
		Transform hit = null;
		Queue<Transform> hits = new Queue<Transform>();
		void UpdateRaycastHit(Transform rayHit) {
			hits.Enqueue(rayHit);
		}

		player.hand.input.clickEvent += UpdateRaycastHit;

		yield return new WaitUntil(delegate { return waiting; });
		waiting = false;

		//stop mouse from working immediately
		if (ServerManager.CheckIfClient(player, true)) {
			player.hand.input.ActivateSpellMode();
		}

		yield return new WaitForSeconds(0.25f);

		Vector3 endPos = Vector3.up * 0.25f;

		//do spell thing
		int newIndex = placement.index;
		PlayerData target = null;
		while (target == null) {
			yield return eof;
			if (transform.localPosition != endPos)
				transform.localPosition = Vector3.MoveTowards(transform.localPosition,
					endPos, 2f * Time.deltaTime);

			if (hits.Count > 0)
				hit = hits.Dequeue();

			target = ((SpellData)data).targetting.Invoke(current, opposing, ref newIndex, ref hit);
		}

		hits.Clear();
		hits = null;

		while (!moving && transform.localPosition != placement.floatingHeight) {
			transform.localPosition = Vector3.MoveTowards(transform.localPosition,
				placement.floatingHeight, 4f * Time.deltaTime);
			yield return eof;
		}

		//relink mouse functions
		player.hand.input.clickEvent -= UpdateRaycastHit;

		//can't target something, so just drop card
		if (newIndex < -1) {
			//relink mouse functions
			Release(true);
			//return the mana cost
			current.ReduceMana(-data.cost);

			if (ServerManager.CheckIfClient(player, true)) {
				player.hand.input.DeactivateSpellMode(true);
			}
			
			yield break;
		}

		//now we can render the face
		base.OnPlace(current, opposing);
		
		//just in case
		moving = true;

		//cast spell should take card of this
		((SpellData)data).CastSpell(this, target, newIndex);
	}

	public void SelfDestruct(float delay) {
		StartCoroutine(DelayedDeath(delay));
	}

	IEnumerator DelayedDeath(float delay) {
		yield return new WaitForSeconds(delay);

		//delay on client, dont want them clicking button immidiately after the thing
		
		if (ServerManager.CheckIfClient(player, true)) {
			player.hand.input.DeactivateSpellMode(true);
		}
		
		StartCoroutine(Death());
	}
}
