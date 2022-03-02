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
	static public WaitForEndOfFrame eof = new WaitForEndOfFrame();

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public virtual void SetData(CardData newData) {
		data = newData;
		frontFace.material.mainTexture = data.cardArt;

		string cost = "";
		for (int i = data.cost; i > 0; --i) {
			cost += 'o';
		}
		costMesh.text = cost;
	}

	public virtual void OnPlace(int index, GameController.PlayerData current,
		GameController.PlayerData opposing)
	{
		//don't allow default cards to survive
		StartCoroutine("Death");
	}

	IEnumerator Death() {
		if (placement != null)
			placement.UnLink();
		transform.SetParent(null, true);
		for (float i = 1; i >= 0; i -= Time.deltaTime * 2f) {
			transform.localScale = Vector3.one * i;
			yield return eof;
		}
		Destroy(gameObject);
	}
}
