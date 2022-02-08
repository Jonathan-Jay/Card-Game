using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
	public CardHolder placement;
	public CardData data;
	[SerializeField]	protected MeshRenderer frontFace;
	[SerializeField]	protected TMP_Text costMesh;

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public virtual void SetData(CardData newData) {
		data = newData;
		frontFace.material.mainTexture = data.cardArt;

		string cost = "";
		for (int i = data.cost; i >= 0; --i) {
			cost += 'o';
		}
		costMesh.text = cost;
	}

	public virtual void OnPlace() {
		StartCoroutine("Death");
	}

	IEnumerator Death() {
		if (placement != null)
			placement.UnLink();
		transform.SetParent(null, true);
		for (int i = 10; i >= 0; --i) {
			transform.localScale = Vector3.one * i * 0.1f;
			yield return new WaitForSeconds(0.05f);
		}
		Destroy(gameObject);
	}
}
