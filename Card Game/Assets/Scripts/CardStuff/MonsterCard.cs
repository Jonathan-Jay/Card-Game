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
			if (renderingFace) {
				//because of the dirty flag
				renderingFace = false;
				RenderFace();
			}
		}
	}

	public override void SetData(CardData newData) {
		base.SetData(newData);
		targetable = true;
	}

	public override void RenderFace()
	{
		if (renderingFace || !data)	return;

		base.RenderFace();

		SetAttack(((MonsterData)data).attack, Color.black);
		SetHealth(((MonsterData)data).health, Color.black);
	}

	public override void OnPlace(int index, PlayerData current, PlayerData opposing) {
		RenderFace();
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
