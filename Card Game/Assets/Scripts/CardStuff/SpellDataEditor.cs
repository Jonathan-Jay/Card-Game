#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpellData))]
[CanEditMultipleObjects]
public class SpellDataEditor : Editor
{
	static bool lockedDescription = true;
	static bool unlockedInts = true;
	static bool usingRichText = true;
	SerializedProperty cardArt;
	SerializedProperty cardName;
	SerializedProperty cost;
	SerializedProperty flavourText;
	SerializedProperty actionParameter1;
	SerializedProperty actionParameter2;
	SerializedProperty abilityParameter1;
	SerializedProperty abilityParameter2;
	SerializedProperty abilityParameter3;
	SerializedProperty cardDescription;
	SerializedProperty targetting;
	SerializedProperty activate;
	SerializedProperty ability;
	SerializedProperty effect;

	#region TargettingOptions
	struct TargettingOption {
		public string name;
		public string description;
		public string descriptionText;
		//enumName for clarity
		public TargettingOption(string name, string description,
			string descriptionText, SpellData.TargettingOptions enumName)
		{
			this.name = name;
			this.description = description;
			this.descriptionText = descriptionText;
		}
	}

	static TargettingOption[] targettingOptions = {
		new TargettingOption("Opposing Card", "Target the card it's facing (returns to hand if invalid)",
			"the opposing card", SpellData.TargettingOptions.OpposingCard),
		new TargettingOption("Opponent's Field Card", "Target a card of the opposing player's field",
			"one of the active opponent's cards", SpellData.TargettingOptions.OpposingField),
		new TargettingOption("Caster's Field Card", "Target a card of the caster's field",
			"one of the caster's active cards", SpellData.TargettingOptions.SelfField),
		new TargettingOption("Opposing Player", "Target the opposing player directly (make sure to have correct activations though)",
			"the opposing player", SpellData.TargettingOptions.OpposingPlayer),
		new TargettingOption("Player Self", "Target Self (make sure to have correct activations though)",
			"the caster", SpellData.TargettingOptions.PlayerSelf),
		new TargettingOption("Any Card", "Players choose which card to affect, click on the spell to cancel",
			"any card", SpellData.TargettingOptions.TargetAny),
		new TargettingOption("Caster's Card", "Players choose which card of their card to affect, click on the spell to cancel",
			"one of the caster's card", SpellData.TargettingOptions.TargetAnyPlayer),
		new TargettingOption("Opponent's Card", "Players choose which card of the opposing player's card to affect, click on the spell to cancel",
			"one of the opponent's card", SpellData.TargettingOptions.TargetAnyOpposing),
	};
	#endregion

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
			"directly to ", SpellData.ActivationOptions.Direct),
		new ActivationOption("Repeated", "Calls it {0} (actionParameter1) times",
			"<color=green>{0}</color> times to ", SpellData.ActivationOptions.Repeated),
		new ActivationOption("Randomized", "Calls it {0} (actionParameter1) times, hits random opposing cards",
			"<color=green>{0}</color> times to random ", SpellData.ActivationOptions.Randomized),
		new ActivationOption("EveryCard", "Hits Every Card, put actionParameter1 > 0 for backrow inclusion",
			"every card ", SpellData.ActivationOptions.Everything),
	};
	#endregion
	
	#region AbilityOptions
	struct AbilityOption {
		public string name;
		public string description;
		//{2} gets replaced with abilityP1
		//{3} gets replaced with abilityP2
		//{4} gets replaced with abilityP3
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
		new AbilityOption("Instant Kill", "Kill the target with no overkill",
			"Destroy ", SpellData.AbilityOptions.Kill),
		new AbilityOption("Boost stats", "temporarily boost a monster's stats by {3} hp (abilityParameter2) and {4} (abilityParameter3) attack for {2} (abilityParameter1) turns (<= 0 is infinite)",
			"Boost hp by <color=red>{3}</color> and attack by <color=red>{4}</color> for <color=green>{2}</color> turns of ", SpellData.AbilityOptions.Boost),
		new AbilityOption("Remove mana", "Remove {2} (abilityParameter1) mana from the opponent",
			"Lose <color=yellow>{2}</color> mana ", SpellData.AbilityOptions.StealMana),
		new AbilityOption("Draw Card", "Target player draws {2} (abilityParameter1) cards",
			"Draw <color=yellow>{2}</color> cards ", SpellData.AbilityOptions.DrawCard),
	};
	#endregion

	private void OnEnable() {
		cardName = this.serializedObject.FindProperty("cardName");
		cardArt = this.serializedObject.FindProperty("cardArt");
		cost = this.serializedObject.FindProperty("cost");
		flavourText = this.serializedObject.FindProperty("flavourText");

		actionParameter1 = this.serializedObject.FindProperty("actionParameter1");
		actionParameter2 = this.serializedObject.FindProperty("actionParameter2");
		abilityParameter1 = this.serializedObject.FindProperty("abilityParameter1");
		abilityParameter2 = this.serializedObject.FindProperty("abilityParameter2");
		abilityParameter3 = this.serializedObject.FindProperty("abilityParameter3");
		cardDescription = this.serializedObject.FindProperty("cardDescription");
		
		targetting = this.serializedObject.FindProperty("targettingOption");
		activate = this.serializedObject.FindProperty("activateOption");
		ability = this.serializedObject.FindProperty("abilityOption");

		effect = this.serializedObject.FindProperty("effect");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();
		GUIStyle richText = new GUIStyle(GUI.skin.label);
		richText.richText = usingRichText;
		richText.wordWrap = true;
		GUIStyle bigText = new GUIStyle(GUI.skin.label);
		bigText.richText = usingRichText;
		bigText.wordWrap = true;
		bigText.stretchHeight = true;
		bigText.fixedWidth = EditorGUIUtility.currentViewWidth - 50f;
		GUIStyle richTextBoxed = new GUIStyle(GUI.skin.textField);
		richTextBoxed.richText = usingRichText;
		richTextBoxed.wordWrap = true;
		richTextBoxed.stretchHeight = true;

		cardName.stringValue = EditorGUILayout.TextField("Card Name", cardName.stringValue, richTextBoxed);
		EditorGUILayout.PropertyField(cardArt);
		
		//if multiediting, can add other safeties, but whatevs
		if (cardName.hasMultipleDifferentValues) {
			EditorGUILayout.PropertyField(actionParameter1);
			EditorGUILayout.PropertyField(actionParameter2);
			EditorGUILayout.PropertyField(abilityParameter1);
			EditorGUILayout.PropertyField(abilityParameter2);
			EditorGUILayout.PropertyField(abilityParameter3);
			EditorGUILayout.PropertyField(cardDescription);
			EditorGUILayout.PropertyField(effect);
			EditorGUILayout.PropertyField(targetting);
			EditorGUILayout.PropertyField(activate);
			EditorGUILayout.PropertyField(ability);

			this.serializedObject.ApplyModifiedProperties();
			return;
		}

		unlockedInts = EditorGUILayout.Toggle("Unlock Sliders", unlockedInts);

		bool dirty = false;

		if (unlockedInts){
			cost.intValue = EditorGUILayout.IntField("Cost", cost.intValue);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(actionParameter1);
			EditorGUILayout.PropertyField(actionParameter2);
			EditorGUILayout.PropertyField(abilityParameter1);
			EditorGUILayout.PropertyField(abilityParameter2);
			EditorGUILayout.PropertyField(abilityParameter3);
		}
		else {
			cost.intValue = EditorGUILayout.IntSlider("Cost", cost.intValue, 0, 5);

			EditorGUI.BeginChangeCheck();

			actionParameter1.intValue = EditorGUILayout.IntSlider("Action Parameter 1",
				actionParameter1.intValue, 0, 10);
			actionParameter2.intValue = EditorGUILayout.IntSlider("Action Parameter 2",
				actionParameter2.intValue, 0, 10);
			abilityParameter1.intValue = EditorGUILayout.IntSlider("Ability Parameter 1",
				abilityParameter1.intValue, -10, 10);
			abilityParameter2.intValue = EditorGUILayout.IntSlider("Ability Parameter 2",
				abilityParameter2.intValue, -10, 10);
			abilityParameter3.intValue = EditorGUILayout.IntSlider("Ability Parameter 3",
				abilityParameter3.intValue, -10, 10);
		}

		dirty = EditorGUI.EndChangeCheck();

		EditorGUILayout.LabelField("<b>Description:</b>", richText);
		++EditorGUI.indentLevel;
		cardDescription.stringValue = EditorGUILayout.TextArea(cardDescription.stringValue, richTextBoxed);
		--EditorGUI.indentLevel;
		lockedDescription = EditorGUILayout.Toggle("Lock Description", lockedDescription);



		//do spell options here
		EditorGUILayout.Space();
		EditorGUI.BeginChangeCheck();




		//ability;
		EditorGUILayout.PropertyField(ability);
		//display text
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Name: <b>" + abilityOptions[ability.enumValueIndex].name + "</b>", richText);
		EditorGUILayout.LabelField(abilityOptions[ability.enumValueIndex].description
			.Replace("{2}", abilityParameter1.intValue.ToString())
			.Replace("{3}", abilityParameter2.intValue.ToString())
			.Replace("{4}", abilityParameter3.intValue.ToString()), bigText);
		--EditorGUI.indentLevel;
		EditorGUILayout.Space();

		//activation
		EditorGUILayout.PropertyField(activate);
		//display text
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Name: <b>" + activationOptions[activate.enumValueIndex].name + "</b>", richText);
		EditorGUILayout.LabelField(activationOptions[activate.enumValueIndex].description
			.Replace("{0}", actionParameter1.intValue.ToString())
			.Replace("{1}", actionParameter2.intValue.ToString()), bigText);
		--EditorGUI.indentLevel;
		EditorGUILayout.Space();

		//targetting
		EditorGUILayout.PropertyField(targetting);
		//display text
		++EditorGUI.indentLevel;
		EditorGUILayout.LabelField("Name: <b>" + targettingOptions[targetting.enumValueIndex].name + "</b>", richText);
		EditorGUILayout.LabelField(targettingOptions[targetting.enumValueIndex].description);
		--EditorGUI.indentLevel;
		EditorGUILayout.Space();

		if (!lockedDescription && (dirty || EditorGUI.EndChangeCheck())) {
			//change description
			cardDescription.stringValue = abilityOptions[ability.enumValueIndex].descriptionText
				+ activationOptions[activate.enumValueIndex].descriptionText
				+ targettingOptions[targetting.enumValueIndex].descriptionText;
			cardDescription.stringValue = cardDescription.stringValue
				.Replace("{0}", actionParameter1.intValue.ToString())
				.Replace("{1}", actionParameter2.intValue.ToString())
				.Replace("{2}", abilityParameter1.intValue.ToString())
				.Replace("{3}", abilityParameter2.intValue.ToString())
				.Replace("{4}", abilityParameter3.intValue.ToString());
		}

		EditorGUILayout.LabelField("Flavour Text");
		flavourText.stringValue = EditorGUILayout.TextArea(flavourText.stringValue, richTextBoxed);

		usingRichText = EditorGUILayout.Toggle("Use Rich Text", usingRichText);
		//prefab entry
		EditorGUILayout.PropertyField(effect);

		this.serializedObject.ApplyModifiedProperties();
	}
}
#endif