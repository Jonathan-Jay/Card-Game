using UnityEngine;
using UnityEditor;

//helps shorten things
using AbilityFunc = System.Action<Card, SpellData>;
using ActivationFunc = System.Action<GameController.PlayerData, int, System.Action<Card, SpellData>, SpellData>;

[CustomEditor(typeof(SpellData))]
public class SpellDataEditor : Editor
{
	struct ActivationOption {
		public string name;
		public string description;
		//{0} gets replaced with actionP
		//{1} gets replaced with AbilityP1
		//{2} gets replaced with AbilityP2
		public string descriptionText;
		public ActivationFunc func;
		public ActivationOption(string name, string description,
			string descriptionText, ActivationFunc func) {
				this.name = name;
				this.description = description;
				this.descriptionText = descriptionText;
				this.func = func;
			}
	}

	int activationIndex = 0;
	ActivationOption[] activationOptions = {
		new ActivationOption("Direct", "Just calls it once on the target",
			"directly", SpellData.DirectActivation),
		new ActivationOption("Repeated", "Calls it {0} (actionParameter1) times",
			"<color=green>{0}</color> times", SpellData.RepeatedActivation),
	};

	public override void OnInspectorGUI()
	{
		GUIStyle richText = new GUIStyle(GUI.skin.label);
		richText.richText = true;

		SpellData data = target as SpellData;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Card Art");
		data.cardArt = EditorGUILayout.ObjectField(data.cardArt, typeof(Texture2D), false) as Texture2D;
		EditorGUILayout.EndHorizontal();
		data.cost = EditorGUILayout.IntSlider("Cost", data.cost, 0, 5);
		
		bool dirty = false;
		int temp;

		temp = data.actionParameter1;
		data.actionParameter1 = EditorGUILayout.IntSlider("Action Parameter 1", data.actionParameter1, 0, 10);
		dirty = temp != data.actionParameter1 || dirty;
		
		temp = data.actionParameter2;
		data.actionParameter2 = EditorGUILayout.IntSlider("Action Parameter 2", data.actionParameter2, 0, 10);
		dirty = temp != data.actionParameter2 || dirty;

		temp = data.abilityParameter1;
		data.abilityParameter1 = EditorGUILayout.IntSlider("Ability Parameter 1", data.abilityParameter1, 0, 10);
		dirty = temp != data.abilityParameter1 || dirty;

		temp = data.abilityParameter2;
		data.abilityParameter2 = EditorGUILayout.IntSlider("Ability Parameter 2", data.abilityParameter2, 0, 10);
		dirty = temp != data.abilityParameter2 || dirty;

		EditorGUILayout.LabelField("<b>Description:</b>", richText);
		++EditorGUI.indentLevel;
		data.cardDescription = EditorGUILayout.TextField(data.cardDescription, richText);
		--EditorGUI.indentLevel;

		//do spell options here
		temp = activationIndex;
		activationIndex = EditorGUILayout.IntSlider("Choice", activationIndex, 0, activationOptions.Length - 1);
		dirty = temp != activationIndex || dirty;

		//display text
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Name: <b>" + activationOptions[activationIndex].name + "</b>", richText);
		EditorGUILayout.LabelField(activationOptions[activationIndex].description
			.Replace("{0}", data.actionParameter1.ToString()).Replace("{1}", data.actionParameter2.ToString()));
		--EditorGUI.indentLevel;



		if (dirty) {
			data.activate = activationOptions[activationIndex].func;
			data.cardDescription = "{2}, {3}, " + activationOptions[activationIndex].descriptionText;
			data.cardDescription = data.cardDescription.Replace("{0}", data.actionParameter1.ToString());
			data.cardDescription = data.cardDescription.Replace("{1}", data.actionParameter2.ToString());
			data.cardDescription = data.cardDescription.Replace("{2}", data.abilityParameter1.ToString());
			data.cardDescription = data.cardDescription.Replace("{3}", data.abilityParameter2.ToString());
		}
	}
}
