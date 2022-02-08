using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonsterCard : Card
{
	[SerializeField] protected TMP_Text attackMesh;
	[SerializeField] protected TMP_Text healthMesh;
	public int currHealth = 0;
	public int currAttack = 0;

	private void Start() {
		if (data != null) {
			SetData(data);
		}
	}

	public override void SetData(CardData newData) {
		base.SetData(newData);
		currAttack = ((MonsterData)newData).attack;
		attackMesh.text = currAttack.ToString();

		currHealth = ((MonsterData)newData).health;
		healthMesh.text = currHealth.ToString();
		healthMesh.color = Color.black;
	}

	public override void OnPlace() {
		
	}

	//returns overkill
	public int Attack(MonsterCard target) {
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
}
