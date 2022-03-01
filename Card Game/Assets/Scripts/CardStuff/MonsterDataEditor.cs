using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterData))]
public class MonsterDataEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MonsterData data = target as MonsterData;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Card Art");
		data.cardArt = EditorGUILayout.ObjectField(data.cardArt, typeof(Texture2D), false) as Texture2D;
		EditorGUILayout.EndHorizontal();
		data.cost = EditorGUILayout.IntSlider("Cost", data.cost, 0, 10);
		data.health = EditorGUILayout.IntSlider("Health", data.health, 1, 10);
		data.attack = EditorGUILayout.IntSlider("Attack", data.attack, 0, 10);
	}
}
