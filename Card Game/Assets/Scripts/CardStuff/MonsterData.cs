using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "CardData/MonsterData", order = 0)]
public class MonsterData : CardData {
	public int health;
	public int attack;
	public override bool CheckCost(PlayerData player)
	{
		return true;
	}
}
