#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterData))]
public class MonsterDataEditor : Editor
{
	SerializedProperty cardName;
	SerializedProperty cardArt;
	SerializedProperty cost;
	SerializedProperty health;
	SerializedProperty attack;

	SerializedProperty healthRMax;
	SerializedProperty attackRMax;
	SerializedProperty random;

	private void OnEnable() {
		cardName = this.serializedObject.FindProperty("cardName");
		cardArt = this.serializedObject.FindProperty("cardArt");
		cost = this.serializedObject.FindProperty("cost");
		health = this.serializedObject.FindProperty("health");
		attack = this.serializedObject.FindProperty("attack");
		random = this.serializedObject.FindProperty("random");
		healthRMax = this.serializedObject.FindProperty("healthRMax");
		attackRMax = this.serializedObject.FindProperty("attackRMax");
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
		cost.intValue = EditorGUILayout.IntSlider("Cost", cost.intValue, 0, 4);
		attack.intValue = EditorGUILayout.IntSlider("Attack", attack.intValue, 0, 10);
		health.intValue = EditorGUILayout.IntSlider("Health", health.intValue, 0, 10);
		random.boolValue = EditorGUILayout.Toggle("Randomized", random.boolValue);
		if (random.boolValue) {
			attackRMax.intValue = EditorGUILayout.IntSlider("Attack Randomized Max", attackRMax.intValue, 0, 10);
			healthRMax.intValue = EditorGUILayout.IntSlider("Health Randomized Max", healthRMax.intValue, 0, 10);
		}

		this.serializedObject.ApplyModifiedProperties();
	}
}
#endif