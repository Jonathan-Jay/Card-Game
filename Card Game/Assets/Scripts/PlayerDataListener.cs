using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDataListener : MonoBehaviour
{
	[HideInInspector]
	public PlayerData target = null;
	[SerializeField] TMP_Text healthNumber;
	[SerializeField] List<TMP_Text> healthNumbers;
	int healthIndex = 0;
	[SerializeField] TMP_Text manaNumber;
	[SerializeField] List<TMP_Text> manaNumbers;
	int manaIndex = 0;

	private void Awake() {
		//hide the numbers
		foreach (TMP_Text num in healthNumbers) {
			num.gameObject.SetActive(false);
		}
		foreach (TMP_Text num in manaNumbers) {
			num.gameObject.SetActive(false);
		}
	}

	private void OnEnable() {
		if (target != null) {
			target.healthUpdated += UpdateHealth;
			target.manaUpdated += UpdateMana;
		}
	}

	private void OnDisable() {
		if (target != null) {
			target.healthUpdated -= UpdateHealth;
			target.manaUpdated -= UpdateMana;
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
		healthIndex = ++healthIndex % healthNumbers.Count;

		healthNumber.text = target.currentHP.ToString();
	}

	void UpdateMana(int prevMana) {
		//only update if it exists
		if (!manaNumber)	return;

		manaNumbers[manaIndex].text = (target.currentMana - prevMana).ToString("+#;-#;0");
		StartCoroutine(AnimateObject(manaNumbers[manaIndex].transform,
			Vector3.up * 0.2f + Vector3.right * 0.1f, Vector3.up * 0.4f + Vector3.right * 0.1f));
		manaIndex = ++manaIndex % manaNumbers.Count;

		manaNumber.text = target.currentMana.ToString();
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
}
