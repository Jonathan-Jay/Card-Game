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
	static public WaitForEndOfFrame eof = new WaitForEndOfFrame();

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

	public virtual void SetData(CardData newData) {
		data = newData;
		data.Init();
	}

	public virtual void RenderFace() {
		if (renderingFace || !data)	return;

		nameMesh.text = data.cardName;

		frontFace.material.mainTexture = data.cardArt;

		costMesh.text = "Lv. " + (data.cost  + 1);

		//dirty flag
		renderingFace = true;
	}

	public virtual void OnPlace(PlayerData current, PlayerData opposing) {
		//don't allow default cards to survive
		RenderFace();
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
		if (transform.parent != player.hand.transform) {
			transform.SetParent(player.hand.transform, true);
			StartCoroutine(ReturnToHand());
		}
	}

	IEnumerator Death() {
		Release();
		for (float i = 1; i >= 0; i -= Time.deltaTime * 2f) {
			transform.localScale = Vector3.one * i;
			yield return eof;
		}
		Destroy(gameObject);
	}

	IEnumerator ReturnToHand() {
		gameObject.layer = player.hand.input.ignoredLayer;

		float returnSpeed = 2f;
		float returnRotSpeed = 135f;
		GetComponent<Rigidbody>().isKinematic = true;

		//return the card to the hand, do somethign proper next time
		int numInHands = player.hand.transform.childCount;
		Vector3 targetPos = Vector3.left * 1.2f + Vector3.forward * 0.3f;
		while (numInHands > 5) {
			targetPos += Vector3.back * 0.2f + Vector3.up * 0.1f;
			numInHands -= 5;
		}
		targetPos += Vector3.right * numInHands * 0.4f;
		//Quaternion targetRot = Quaternion.identity;
		Quaternion targetRot = Quaternion.Euler(0f, 0f, 10f);

		while (transform.parent == player.hand.transform)
		{
			if (Vector3.Distance(transform.localPosition, targetPos) > 0.1f) {
				transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos,
					returnSpeed * Time.deltaTime);
			}
			else {
				transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos,
					returnSpeed * Time.deltaTime);
			}

			if (transform.localRotation != targetRot) {
			//if (Quaternion.Angle(transform.localRotation, targetRot) > 1f) {
			//	transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot,
			//		returnSpeed * Time.deltaTime);
			//}
			//else {
				transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot,
					returnRotSpeed * Time.deltaTime);
			}

			if (transform.localPosition == targetPos && transform.localRotation == targetRot){
				break;
			}
			yield return eof;
		}

		//ensure transform is good
		if (transform.parent == player.hand.transform) {
			transform.localPosition = targetPos;
			transform.localRotation = targetRot;
		}

		gameObject.layer = player.hand.input.cardLayer;
	}
}
