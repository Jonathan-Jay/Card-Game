using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "CardData/MonsterData", order = 0)]
public class MonsterData : CardData {
	public int health;
	public int attack;

	public int healthRMax;
	public int attackRMax;
	public bool random = false;
	public AudioClip attackSound;

	public override bool CheckCost(PlayerData player)
	{
		int monsterCount = 0;
		for (int i = 0; i < player.field.Count; ++i) {
			if (player.field[i].holding && player.field[i].holding.targetable) {
				++monsterCount;
			}
			if (player.backLine[i].holding && player.backLine[i].holding.targetable) {
				++monsterCount;
			}
		}

		//check if enough monsters to sacrifice
		return monsterCount >= cost;
	}
}
