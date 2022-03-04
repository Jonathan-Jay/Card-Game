using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
	public CardHolder placement;
	public CardData data;
	public HandManager hand;
	[HideInInspector]	public bool targettable = false;
	[SerializeField]	protected MeshRenderer frontFace;
	[SerializeField]	protected TMP_Text nameMesh;
	[SerializeField]	protected TMP_Text costMesh;
	static public WaitForEndOfFrame eof = new WaitForEndOfFrame();

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public virtual void SetData(CardData newData) {
		data = newData;
		data.Init();
		nameMesh.text = data.cardName;

		frontFace.material.mainTexture = data.cardArt;

		string cost = "";
		for (int i = data.cost; i > 0; --i) {
			cost += 'o';
		}
		costMesh.text = cost;
	}

	public virtual void OnPlace(int index, PlayerData current, PlayerData opposing) {
		//don't allow default cards to survive
		StartCoroutine("Death");
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
		if (transform.parent != hand.transform) {
			transform.SetParent(hand.transform, true);
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
		int tempLayer = gameObject.layer;
		gameObject.layer = hand.input.ignoredLayer;

		float returnSpeed = 2f;
		float returnRotSpeed = 15f;
		GetComponent<Rigidbody>().isKinematic = true;

		//return the card to the hand, do somethign proper next time
		Vector3 targetPos = Vector3.zero;
		Quaternion targetRot = Quaternion.identity;

		while (transform.parent == hand.transform)
		{
			if (Vector3.Distance(transform.localPosition, targetPos) > 0.25f) {
				transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos,
					returnSpeed * Time.deltaTime);
			}
			else {
				transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos,
					returnSpeed * Time.deltaTime);
			}

			if (Quaternion.Angle(transform.localRotation, targetRot) > 1f) {
				transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot,
					returnRotSpeed * Time.deltaTime);
			}
			else {
				transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot,
					returnRotSpeed * Time.deltaTime);
			}

			if (transform.localPosition == targetPos && transform.localRotation == targetRot){
				break;
			}
			yield return eof;
		}

		//ensure transform is good
		transform.localPosition = targetPos;
		transform.localRotation = targetRot;

		gameObject.layer = tempLayer;
	}
}
