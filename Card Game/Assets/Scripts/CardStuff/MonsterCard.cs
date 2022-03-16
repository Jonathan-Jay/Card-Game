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
	public class TempEffect
	{
		public int duration;
		public int hpBoost;
		public int atkBoost;
		public TempEffect(int duration, int hpBoost, int atkBoost)
		{
			this.duration = duration;
			this.hpBoost = hpBoost;
			this.atkBoost = atkBoost;
		}
	}
	List<TempEffect> boosts = new List<TempEffect>();

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

		if (((MonsterData)data).random) {
			currAttack = currHealth = int.MaxValue;
		}
		else {
			currAttack = ((MonsterData)data).attack;
			currHealth = ((MonsterData)data).health;
		}
		attackMesh.color = Color.black;
		healthMesh.color = Color.black;
	}

	public override void RenderFace()
	{
		if (renderingFace || !data)	return;

		base.RenderFace();

		//randomized right now
		if (currAttack == int.MaxValue) {
			attackMesh.text = "?";
			healthMesh.text = "?";
		}
		else {
			//manually do it to not reset colours
			attackMesh.text = Mathf.Max(currAttack, 0).ToString();
			healthMesh.text = currHealth.ToString();
		}

		//SetAttack(((MonsterData)data).attack, Color.black);
		//SetHealth(((MonsterData)data).health, Color.black);
	}

	public override void HideFace()
	{
		if (!renderingFace)	return;

		base.HideFace();

		healthMesh.text = "";
		attackMesh.text = "";
	}

	public override void OnPlace(PlayerData current, PlayerData opposing)
	{
		//calc stats
		MonsterData monData = (MonsterData)data;
		if (currAttack == int.MaxValue) {
			SetAttack(Random.Range(monData.attack, monData.attackRMax), Color.black);
			SetHealth(Random.Range(monData.health, monData.healthRMax), Color.black);
		}

		//render face
		base.OnPlace(current, opposing);

		//perform cost check if cost is not zero
		if (data.cost > 0) {
			//Ask for sacrifices
			StartCoroutine(CheckCost(current, opposing));
		}
	}

	IEnumerator CheckCost(PlayerData current, PlayerData opposing) {
		RaycastHit hit = new RaycastHit();
		void UpdateRaycastHit(RaycastHit rayHit) {
			hit = rayHit;
		}

		current.hand.input.ActivateSpellMode();
		current.hand.input.clickEvent += UpdateRaycastHit;

		for (float i = 0; i < 0.25f; i += Time.deltaTime) {
			transform.localPosition += Vector3.forward * Time.deltaTime;
			yield return eof;
		}
		transform.localPosition = placement.floatingHeight + Vector3.forward * 0.25f;

		int requirement = data.cost;
		int index = placement.index;
		PlayerData target = null;
		List<int> targets = new List<int>();

		while (requirement > 0) {
			yield return eof;
			target = SpellData.TargetAnyPlayerCard(current, opposing, ref index, ref hit);

			//if you didnt click something
			if (!target) continue;

			//clicked on a card, note down specific card and reduce requirement
			if (index >= 0) {
				//reject duplicates
				if (!targets.Contains(index)) {
					current.field[index].holding.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
					targets.Add(index);
					--requirement;
				}
				index = placement.index;
				hit = new RaycastHit();
			}
			//clicked on self, cancel the thing
			else {
				requirement = -1;
			}
			//reset
		}

		//cancelled
		if (requirement < 0) {
			//revert transforms of selected cards
			for (int i = 0; i < targets.Count; ++i) {
				current.field[targets[i]].holding.transform.localRotation = Quaternion.identity;
			}
			Release();
		}
		else {
			//kill all selected cards
			MonsterCard monster;
			for (int i = 0; i < targets.Count; ++i) {
				monster = (MonsterCard)current.field[targets[i]].holding;
				monster.TakeDamage(monster.currHealth);
			}

			for (float i = 0; i < 0.25f; i += Time.deltaTime) {
				transform.localPosition += Vector3.back * Time.deltaTime;
				yield return eof;
			}
			transform.localPosition = placement.floatingHeight;
		}

		//give controls back whether or not it succeeded
		current.hand.input.clickEvent -= UpdateRaycastHit;
		current.hand.input.DeactivateSpellMode();
	}

	public void Boost(TempEffect boostEffect) {
		if (boostEffect.atkBoost != 0) {
			//other boosts should already be applied
			SetAttack(currAttack += boostEffect.atkBoost,
				currAttack > ((MonsterData)data).attack ? Color.green : Color.red);
		}
		if (boostEffect.hpBoost != 0) {
			//other boosts should already be applied
			SetHealth(currHealth += boostEffect.hpBoost, currHealth > ((MonsterData)data).health ? Color.green : Color.red);
			//check if they die from this
			if (currHealth <= 0) {
				StartCoroutine("Death");
				return;
			}
		}
		boosts.Add(boostEffect);
	}

	//returns overkill, also handles temporary effects
	public int Attack(MonsterCard target) {
		int dmg = 0;
		//dont attack if negative attacks
		if (currAttack > 0)	{
			//if not attacking anything, attacking player directly
			if (target == null) {
				dmg = currAttack;
			}
			else {
				//can do this if overkill damage
				//dmg =
				target.TakeDamage(currAttack);
			}
		}

		//handle all boosts after attacking
		if (boosts.Count == 0) return dmg;

		MonsterData monData = (MonsterData)data;
		int newAttack = currAttack;
		int newHealth = currHealth;
		for (int i = 0; i < boosts.Count;) {
			//if expired, remove buff
			TempEffect temp = boosts[i];
			if (--(temp.duration) == 0) {
				newAttack -= temp.atkBoost;
				newHealth -= temp.hpBoost;
				boosts.RemoveAt(i);
			}
			else {	++i;	}
		}
		if (newAttack != currAttack) {
			SetAttack(newAttack, newAttack == monData.attack ? Color.black :
				(currAttack > monData.attack ? Color.green : Color.red));
		}
		if (newHealth != currHealth) {
			//don't let them go below max
			newHealth = Mathf.Max(monData.health, newHealth);

			SetHealth( newHealth, newHealth == monData.health ? Color.black :
				(currHealth > monData.health ? Color.green : Color.red));
			if (newHealth <= 0) {
				StartCoroutine("Death");
			}
		}

		return dmg;
	}

	//also resets colour
	public void SetAttack(int newValue, Color colour) {
		currAttack = newValue;
		//don't allow negative attack
		if (currAttack < 99)
			attackMesh.text = Mathf.Max(0, currAttack).ToString();
		else
			attackMesh.text = "∞";
		attackMesh.color = colour;
	}

	public void SetHealth(int newValue, Color colour) {
		currHealth = newValue;
		if (currHealth < 99)
			healthMesh.text = currHealth.ToString();
		else
			healthMesh.text = "∞";
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
