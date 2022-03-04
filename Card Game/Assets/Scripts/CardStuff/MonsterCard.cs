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
		SetAttack(((MonsterData)newData).attack, Color.black);
		SetHealth(((MonsterData)newData).health, Color.black);
		targettable = true;
	}

	public override void OnPlace(int index, PlayerData current, PlayerData opposing) {
		
	}

	//returns overkill
	public int Attack(MonsterCard target) {
		int dmg = target.TakeDamage(currAttack);
		
		//reset attack if boosted
		if (attackMesh.color != Color.black)
			SetAttack(((MonsterData)data).attack, Color.black);
		return dmg;
	}

	//also resets colour
	public void SetAttack(int newValue, Color colour) {
		currAttack = newValue;
		attackMesh.text = currAttack.ToString();
		attackMesh.color = colour;
	}

	public void SetHealth(int newValue, Color colour) {
		currHealth = newValue;
		healthMesh.text = currHealth.ToString();
		healthMesh.color = colour;
	}

	//returs overkill
	public int TakeDamage(int amt) {
		if (currHealth <= 0)	return 0;
		SetHealth(currHealth - amt, Color.red);
		if (currHealth <= 0) {
			//queue death here
			StartCoroutine("Death");
			return -currHealth;
		}
		return -1;
	}
}
