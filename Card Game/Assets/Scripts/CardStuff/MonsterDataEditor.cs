#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterData))]
[CanEditMultipleObjects]
public class MonsterDataEditor : Editor
{
	static bool unlockedInts = false;
	SerializedProperty cardName;
	SerializedProperty cardArt;
	SerializedProperty cost;
	SerializedProperty health;
	SerializedProperty attack;
	SerializedProperty flavourText;

	SerializedProperty healthRMax;
	SerializedProperty attackRMax;
	SerializedProperty random;
	SerializedProperty attackSound;

	private void OnEnable() {
		cardName = this.serializedObject.FindProperty("cardName");
		cardArt = this.serializedObject.FindProperty("cardArt");
		cost = this.serializedObject.FindProperty("cost");
		flavourText = this.serializedObject.FindProperty("flavourText");

		health = this.serializedObject.FindProperty("health");
		attack = this.serializedObject.FindProperty("attack");
		random = this.serializedObject.FindProperty("random");
		healthRMax = this.serializedObject.FindProperty("healthRMax");
		attackRMax = this.serializedObject.FindProperty("attackRMax");
		attackSound = this.serializedObject.FindProperty("attackSound");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();
		GUIStyle richText = new GUIStyle(GUI.skin.textField);
		richText.richText = true;
		richText.wordWrap = true;
		richText.stretchHeight = true;

		cardName.stringValue = EditorGUILayout.TextField("Card Name", cardName.stringValue, richText);
		EditorGUILayout.PropertyField(cardArt);

		unlockedInts = EditorGUILayout.Toggle("Unlock Sliders", unlockedInts);

		//don't show when multi editing, it breaks it
		if (unlockedInts || cardName.hasMultipleDifferentValues) {
			EditorGUILayout.PropertyField(cost);
			EditorGUILayout.PropertyField(attack);
			EditorGUILayout.PropertyField(health);
			EditorGUILayout.PropertyField(random);
			if (random.boolValue) {
				EditorGUILayout.PropertyField(attackRMax);
				EditorGUILayout.PropertyField(healthRMax);
			}
		}
		else {
			cost.intValue = EditorGUILayout.IntSlider("Cost", cost.intValue, 0, 4);
			attack.intValue = EditorGUILayout.IntSlider("Attack", attack.intValue, 0, 10);
			health.intValue = EditorGUILayout.IntSlider("Health", health.intValue, 0, 10);
			random.boolValue = EditorGUILayout.Toggle("Randomized", random.boolValue);
			if (random.boolValue) {
				attackRMax.intValue = EditorGUILayout.IntSlider("Attack Randomized Max", attackRMax.intValue, 0, 10);
				healthRMax.intValue = EditorGUILayout.IntSlider("Health Randomized Max", healthRMax.intValue, 0, 10);
			}
		}
		flavourText.stringValue = EditorGUILayout.TextField("Flavour Text", flavourText.stringValue, richText);
		
		EditorGUILayout.PropertyField(attackSound);

		this.serializedObject.ApplyModifiedProperties();
	}
}
#endif