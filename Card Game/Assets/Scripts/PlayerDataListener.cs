using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDataListener : MonoBehaviour
{
	//because coroutines cant have references
	public class IntByRef {
		public int val = -2;
		public static bool operator<(IntByRef a, int b) {	return a.val < b;	}
		public static bool operator>(IntByRef a, int b) {	return a.val > b;	}
		public static bool operator==(IntByRef a, int b) {	return a.val == b;	}
		public static bool operator!=(IntByRef a, int b) {	return a.val != b;	}

		//not bothering lol, I just wanted to get rid of the warning
		public override bool Equals(object obj) {	return false;	}
		//not bothering lol, I just wanted to get rid of the warning
		public override int GetHashCode() {	return 0;	}
	}

	[HideInInspector]
	public PlayerData target = null;
	[SerializeField] TMP_Text healthNumber;
	[SerializeField] TMP_Text[] healthNumbers;
	int healthIndex = 0;
	[SerializeField] GameObject[] healthObjects;
	[SerializeField] Vector3 healthDropPos = Vector3.up * 1f;
	[SerializeField] float hpRange = 0.3f;
	IntByRef targetHPCount = new IntByRef();
	[SerializeField] TMP_Text manaNumber;
	[SerializeField] TMP_Text[] manaNumbers;
	int manaIndex = 0;
	[SerializeField] GameObject[] manaObjects;
	[SerializeField] Vector3 manaDropPos = Vector3.up * 1f;
	[SerializeField] float manaRange = 0.2f;
	IntByRef targetManaCount = new IntByRef();
	[SerializeField] TMP_Text deckText;

	private void Awake() {
		//hide the numbers
		foreach (TMP_Text num in healthNumbers) {
			num.gameObject.SetActive(false);
		}
		foreach (TMP_Text num in manaNumbers) {
			num.gameObject.SetActive(false);
		}

		//hide the objects
		foreach (GameObject obj in healthObjects) {
			obj.SetActive(false);
		}
		foreach (GameObject obj in manaObjects) {
			obj.SetActive(false);
		}
	}

	private void OnEnable() {
		if (target != null) {
			target.healthUpdated += UpdateHealth;
			target.manaUpdated += UpdateMana;
			target.drawCard += UpdateDeck;
		}
	}

	private void OnDisable() {
		if (target != null) {
			target.healthUpdated -= UpdateHealth;
			target.manaUpdated -= UpdateMana;
			target.drawCard -= UpdateDeck;
		}
	}

	public void SetTarget(PlayerData newTarget) {
		OnDisable();
		target = newTarget;
		OnEnable();
	}

	void UpdateHealth(int prevHealth) {
		//only update if it exists
		if (!healthNumber)	return;

		healthNumbers[healthIndex].text = (target.currentHP - prevHealth).ToString("+#;-#;0");
		StartCoroutine(AnimateObject(healthNumbers[healthIndex].transform,
			Vector3.up * 0.2f + Vector3.right * 0.2f, Vector3.up * 1f + Vector3.right * 0.2f));
		healthIndex = ++healthIndex % healthNumbers.Length;

		healthNumber.text = target.currentHP.ToString();

		//if not iterating, start coroutine
		if (targetHPCount < -1) {
			targetHPCount.val = Mathf.Max(target.currentHP, 0) - 1;
			StartCoroutine(SpawnObjects(healthObjects, targetHPCount, healthDropPos, hpRange));
		}
		//just set the value
		else {
			targetHPCount.val = Mathf.Max(target.currentHP, 0) - 1;
		}
	}

	void UpdateMana(int prevMana) {
		//only update if it exists
		if (!manaNumber)	return;

		manaNumbers[manaIndex].text = (target.currentMana - prevMana).ToString("+#;-#;0");
		StartCoroutine(AnimateObject(manaNumbers[manaIndex].transform,
			Vector3.up * 0.2f + Vector3.right * 0.1f, Vector3.up * 0.4f + Vector3.right * 0.1f));
		manaIndex = ++manaIndex % manaNumbers.Length;

		manaNumber.text = target.currentMana.ToString();

		//if not iterating, start coroutine
		if (targetManaCount < -1) {
			targetManaCount.val = Mathf.Max(target.currentMana, 0) - 1;
			StartCoroutine(SpawnObjects(manaObjects, targetManaCount, manaDropPos, manaRange));
		}
		//just set the value
		else {
			targetManaCount.val = Mathf.Max(target.currentMana, 0) - 1;
		}
	}

	void UpdateDeck() {
		if (!deckText)	return;

		deckText.text = target.canDraw.ToString();

		//also consider changing the material of the deck
	}

	IEnumerator AnimateObject(Transform target, Vector3 startPos, Vector3 endPos) {
		target.gameObject.SetActive(true);

		target.localPosition = startPos;
		while (target.localPosition != endPos) {
			yield return Card.eof;
			target.localPosition = Vector3.MoveTowards(target.localPosition, endPos, Time.deltaTime);
		}
		target.gameObject.SetActive(false);
	}

	IEnumerator SpawnObjects(GameObject[] list, IntByRef count, Vector3 spawnPos, float variation) {
		/*single frame version
		int index = count.val - 1;

		//if valid already, move upwards and remove
		if (list[index].activeInHierarchy) {
			while (list[++index].activeInHierarchy) {
				list[index].SetActive(false);
			}
		}
		//otherwise, build from it
		else {
			while (--index >= 0 && !list[index].activeInHierarchy) {
				list[index].SetActive(true);
				list[index].transform.localPosition = spawnPos
					+ Vector3.right * Random.Range(-variation, variation)
					+ Vector3.forward * Random.Range(-variation, variation);
			}
		}*/

		//some truncator thing
		/*int index = Mathf.Clamp(count.val + 1, 0, list.Length - 1);
		//check if we need to truncate first
		//if count is 1, index 1 should be false, etc.
		if (count < list.Length && list[index].activeInHierarchy) {
			while (index < list.Length && list[index].activeInHierarchy) {
				list[index++].SetActive(false);
			}

			//mark as completed
			count.val = -2;
			yield return null;
		}*/
		
		WaitForSeconds delay = new WaitForSeconds(0.1f);
		//get the index of the first inactive Object
		int index = -1;
		while (++index < list.Length && list[index].activeInHierarchy);

		while (index >= 0 && index < list.Length) {
			//index is the first inactive object

			//first check direction
			if (count == index) {
				//make it active if it isn't already
				if (!list[index].activeInHierarchy) {
					list[index].SetActive(true);
					list[index].transform.localPosition = spawnPos
						+ Vector3.right * Random.Range(-variation, variation)
						+ Vector3.forward * Random.Range(-variation, variation);
				}
				break;
			}
			//we're too small, set active if not already
			else if (count > index) {
				list[index].SetActive(true);
				list[index++].transform.localPosition = spawnPos
					+ Vector3.right * Random.Range(-variation, variation)
					+ Vector3.forward * Random.Range(-variation, variation);
			}
			//we're too big, this is instant
			else if (count < index) {
				while (--index >= 0 && count < index) {
					if (list[index].activeInHierarchy) {
						list[index].SetActive(false);
					}
				}
				//gotta stay as first inactive
				++index;
			}
			yield return delay;
		}

		count.val = -2;
	}
}
