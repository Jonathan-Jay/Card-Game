using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpellData))]
public class SpellDataEditor : Editor
{
	SerializedProperty cardArt;
	SerializedProperty cardName;
	SerializedProperty cost;
	SerializedProperty actionParameter1;
	SerializedProperty actionParameter2;
	SerializedProperty abilityParameter1;
	SerializedProperty abilityParameter2;
	SerializedProperty cardDescription;
	SerializedProperty targetting;
	SerializedProperty activate;
	SerializedProperty ability;

	#region ActivationOptions
	struct ActivationOption {
		public string name;
		public string description;
		//{0} gets replaced with actionP1
		//{1} gets replaced with actionP2
		public string descriptionText;
		//enumName for clarity
		public ActivationOption(string name, string description,
			string descriptionText, SpellData.ActivationOptions enumName) {
				this.name = name;
				this.description = description;
				this.descriptionText = descriptionText;
			}
	}

	static ActivationOption[] activationOptions = {
		new ActivationOption("Direct", "Just calls it once on the target",
			"directly", SpellData.ActivationOptions.Direct),
		new ActivationOption("Repeated", "Calls it {0} (actionParameter1) times",
			"<color=green>{0}</color> times", SpellData.ActivationOptions.Repeated),
		new ActivationOption("Randomized", "Calls it {0} (actionParameter1) times, hits random opposing cards",
			"<color=green>{0}</color> times to random opposing Monsters", SpellData.ActivationOptions.Randomized),
	};
	#endregion
	
	#region AbilityOptions
	struct AbilityOption {
		public string name;
		public string description;
		//{2} gets replaced with abilityP1
		//{3} gets replaced with abilityP2
		public string descriptionText;
		//enumName for clarity
		public AbilityOption(string name, string description,
			string descriptionText, SpellData.AbilityOptions enumName) {
				this.name = name;
				this.description = description;
				this.descriptionText = descriptionText;
			}
	}

	static AbilityOption[] abilityOptions = {
		new AbilityOption("Direct", "Deals {2} (abilityParameter1) damage to the target",
			"Deal <color=red>{2}</color> damage ", SpellData.AbilityOptions.Direct),
		new AbilityOption("Randomized damage", "Deals between {2} (abilityParameter1) and {3} (abilityParameter2) damge to the target",
			"Deal <color=red>{2}</color>-<color=red>{3}</color> damage ", SpellData.AbilityOptions.RandomDamage),
	};
	#endregion

	SpellData.TargettingOptions targettingIndex;
	private void OnEnable() {
		cardName = this.serializedObject.FindProperty("cardName");
		cardArt = this.serializedObject.FindProperty("cardArt");
		cost = this.serializedObject.FindProperty("cost");
		actionParameter1 = this.serializedObject.FindProperty("actionParameter1");
		actionParameter2 = this.serializedObject.FindProperty("actionParameter2");
		abilityParameter1 = this.serializedObject.FindProperty("abilityParameter1");
		abilityParameter2 = this.serializedObject.FindProperty("abilityParameter2");
		cardDescription = this.serializedObject.FindProperty("cardDescription");
		
		targetting = this.serializedObject.FindProperty("targettingOption");
		activate = this.serializedObject.FindProperty("activateOption");
		ability = this.serializedObject.FindProperty("abilityOption");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();
		GUIStyle richText = new GUIStyle(GUI.skin.label);
		richText.richText = true;
		richText.wordWrap = true;
		GUIStyle bigText = new GUIStyle(GUI.skin.label);
		bigText.richText = true;
		bigText.wordWrap = true;
		bigText.stretchHeight = true;
		bigText.fixedWidth = EditorGUIUtility.currentViewWidth - 50f;
		GUIStyle richTextBoxed = new GUIStyle(GUI.skin.textField);
		richTextBoxed.richText = true;
		richTextBoxed.wordWrap = true;
		richTextBoxed.stretchHeight = true;

		cardName.stringValue = EditorGUILayout.TextField("Card Name", cardName.stringValue, richTextBoxed);
		EditorGUILayout.PropertyField(cardArt);
		cost.intValue = EditorGUILayout.IntSlider("Cost", cost.intValue, 0, 5);
		
		bool dirty = false;
		int temp;

		temp = actionParameter1.intValue;
		actionParameter1.intValue = EditorGUILayout.IntSlider("Action Parameter 1", actionParameter1.intValue, 0, 10);
		dirty = temp != actionParameter1.intValue || dirty;
		
		temp = actionParameter2.intValue;
		actionParameter2.intValue = EditorGUILayout.IntSlider("Action Parameter 2", actionParameter2.intValue, 0, 10);
		dirty = temp != actionParameter2.intValue || dirty;

		temp = abilityParameter1.intValue;
		abilityParameter1.intValue = EditorGUILayout.IntSlider("Ability Parameter 1", abilityParameter1.intValue, 0, 10);
		dirty = temp != abilityParameter1.intValue || dirty;

		temp = abilityParameter2.intValue;
		abilityParameter2.intValue = EditorGUILayout.IntSlider("Ability Parameter 2", abilityParameter2.intValue, 0, 10);
		dirty = temp != abilityParameter2.intValue || dirty;

		EditorGUILayout.LabelField("<b>Description:</b>", richText);
		++EditorGUI.indentLevel;
		cardDescription.stringValue = EditorGUILayout.TextField(cardDescription.stringValue, richTextBoxed);
		--EditorGUI.indentLevel;



		//do spell options here
		EditorGUILayout.Space();

		//ability
		temp = ability.enumValueIndex;
		EditorGUILayout.PropertyField(ability);
		dirty = temp != ability.enumValueIndex || dirty;
		//display text
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Name: <b>" + abilityOptions[ability.enumValueIndex].name + "</b>", richText);
		EditorGUILayout.LabelField(abilityOptions[ability.enumValueIndex].description
			.Replace("{2}", abilityParameter1.intValue.ToString())
			.Replace("{3}", abilityParameter2.intValue.ToString()), bigText);
		--EditorGUI.indentLevel;
		EditorGUILayout.Space();

		//activation
		temp = activate.enumValueIndex;
		EditorGUILayout.PropertyField(activate);
		dirty = temp != activate.enumValueIndex || dirty;
		//display text
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Name: <b>" + activationOptions[activate.enumValueIndex].name + "</b>", richText);
		EditorGUILayout.LabelField(activationOptions[activate.enumValueIndex].description
			.Replace("{0}", actionParameter1.intValue.ToString())
			.Replace("{1}", actionParameter2.intValue.ToString()), bigText);
		--EditorGUI.indentLevel;

		if (dirty) {
			//change description
			cardDescription.stringValue = abilityOptions[ability.enumValueIndex].descriptionText
				+ activationOptions[activate.enumValueIndex].descriptionText;
			cardDescription.stringValue = cardDescription.stringValue.Replace("{0}", actionParameter1.intValue.ToString());
			cardDescription.stringValue = cardDescription.stringValue.Replace("{1}", actionParameter2.intValue.ToString());
			cardDescription.stringValue = cardDescription.stringValue.Replace("{2}", abilityParameter1.intValue.ToString());
			cardDescription.stringValue = cardDescription.stringValue.Replace("{3}", abilityParameter2.intValue.ToString());
		}
		this.serializedObject.ApplyModifiedProperties();
	}
}
