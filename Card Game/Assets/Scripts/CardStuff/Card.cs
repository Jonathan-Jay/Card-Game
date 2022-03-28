using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
	public CardHolder placement;
	public CardData data;
	public PlayerData player;
	public bool renderingFace = false;
	[HideInInspector]	public bool targetable = false;
	[SerializeField]	protected MeshRenderer frontFace;
	[SerializeField]	protected TMP_Text nameMesh;
	[SerializeField]	protected TMP_Text costMesh;
	[SerializeField]	protected TMP_Text flavourTextMesh;
	static public WaitForEndOfFrame eof = new WaitForEndOfFrame();
	static Material defaultMaterial = null;
	
	private void Start() {
		if (!defaultMaterial) {
			defaultMaterial = frontFace.material;
		}
		if (data != null) {
			SetData(data);
			if (renderingFace) {
				//because of the dirty flag
				renderingFace = false;
				RenderFace();
			}
		}
	}

	public virtual void SetData(CardData newData) {
		data = newData;
		data.Init();
	}

	public virtual void RenderFace() {
		if (renderingFace || !data)	return;

		nameMesh.text = data.cardName;

		frontFace.material.mainTexture = data.cardArt;

		costMesh.text = "Lv. " + (data.cost  + 1);

		flavourTextMesh.text = data.flavourText;

		//dirty flag
		renderingFace = true;
	}

	public virtual void HideFace() {
		if (!renderingFace) return;

		nameMesh.text = "";

		frontFace.material = defaultMaterial;

		costMesh.text = "";

		flavourTextMesh.text = "";

		renderingFace = false;
	}

	public virtual void OnPlace(PlayerData current, PlayerData opposing) {
		//render most cards
		RenderFace();
		
		//also remove from hand
		current.RemoveCard(this);
	}

	public void Release() {
		if (placement != null) {
			placement.UnLink();
			placement = null;
		}

		if (transform.parent)
			transform.SetParent(null, true);
		GetComponent<Rigidbody>().isKinematic = false;
	}

	public void CallBackCard() {
		if (transform.parent == player.hand.transform) return;
		if (placement)	return;
		
		//put in hand
		player.hand.ReturnCardToHand(transform);
	}

	IEnumerator Death() {
		Release();
		for (float i = 1; i >= 0; i -= Time.deltaTime * 2f) {
			transform.localScale = Vector3.one * i;
			yield return eof;
		}
		Destroy(gameObject);
	}

	//for the children classes:
	public static byte[] animationModeOn = System.Text.Encoding.ASCII.GetBytes("INPANMON");
	public static byte[] animationModeOff = System.Text.Encoding.ASCII.GetBytes("INPANMOF");
	public static byte[] spellModeOn = System.Text.Encoding.ASCII.GetBytes("INPSPLON");
	public static byte[] spellModeOff = System.Text.Encoding.ASCII.GetBytes("INPSPLOF");
}
