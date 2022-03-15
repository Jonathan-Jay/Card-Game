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

	private void OnEnable() {
		cardName = this.serializedObject.FindProperty("cardName");
		cardArt = this.serializedObject.FindProperty("cardArt");
		cost = this.serializedObject.FindProperty("cost");
		health = this.serializedObject.FindProperty("health");
		attack = this.serializedObject.FindProperty("attack");
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
		cost.intValue = EditorGUILayout.IntSlider("Cost", cost.intValue, 0, 3);
		health.intValue = EditorGUILayout.IntSlider("Health", health.intValue, 1, 10);
		attack.intValue = EditorGUILayout.IntSlider("Attack", attack.intValue, 0, 10);

		this.serializedObject.ApplyModifiedProperties();
	}
}
#endif