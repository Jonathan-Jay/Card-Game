using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
	public CardData data;
	[SerializeField]	MeshRenderer frontFace;
	[SerializeField]	TMP_Text attackMesh;
	[SerializeField]	TMP_Text healthMesh;
	[SerializeField]	TMP_Text costMesh;
	public int currHealth = 0;
	public int currAttack = 0;

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public void SetData(CardData newData) {
		data = newData;
		frontFace.material.mainTexture = data.cardArt;
		currAttack = data.attack;
		attackMesh.text = currAttack.ToString();

		currHealth = data.health;
		healthMesh.text = currHealth.ToString();
		healthMesh.color = Color.black;
		string cost = "";
		for (int i = data.cost; i >= 0; --i) {
			cost += 'o';
		}
		costMesh.text = cost;
	}

	//returns overkill
	public int Attack(Card target) {
		return target.TakeDamage(currAttack);
	}

	//returs overkill
	public int TakeDamage(int amt) {
		currHealth -= amt;
		if (currHealth <= 0) {
			healthMesh.text = "0";
			//queue death here
			StartCoroutine("Death");
			return -currHealth;
		}
		healthMesh.text = currHealth.ToString(); 
		return -1;
	}
	IEnumerator Death() {
		for (int i = 10; i >= 0; --i) {
			transform.localScale = Vector3.one * i * 0.1f;
			yield return new WaitForSeconds(0.05f);
		}
		Destroy(gameObject);
	}
}
