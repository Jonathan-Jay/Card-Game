using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	[System.Serializable]
	public class TutorialSection {
		public Transform hoverOver;
		public Vector3 offset = Vector3.up;
		//variables to control what it does
		public string text = "lol";
		public Vector3 scale = Vector3.one;
		public bool canEndTurn = false;
		public bool canPlaceCards = true;

		public enum NextStepTest {
			CLICKTOPROCEED,
			DETECTCARDPLACEMENT,
			DETECTSACRIFICEPLACEMENT,
			CLICKBELL,
			NONE,
			DELAY,
		}

		public NextStepTest stepTest;
		public bool mirrorPrevious = false;
		public int stepTestIndex = -1;

		public enum OpponentAction {
			NONE,
			ENDTURN,
			PLACECARD,
		}

		public OpponentAction aiAction;
		public bool mirrorPlayer = false;
		public bool avoidPlayer = false;
		public int aiIndex = -1;
	}

	public FodderRain lol;
	public List<TutorialSection> sections = new List<TutorialSection>();
	int currentSection = 0;
	public Client client;
	public GameController game;
	public Mouse p1mouse;
	public Mouse p2mouse;
	public bool p1turn = true;

	[SerializeField] KeyCode pauseButton;
	[SerializeField] GameObject pauseScreen;
	[SerializeField] Transform tutorialTrans;
	[SerializeField] Transform tutorialQuad;
	[SerializeField] TMPro.TMP_Text tutorialText;

	SkinnedMeshRenderer p1bell;
	SkinnedMeshRenderer p2bell;
	Color defaultBellCol;

	private void Awake() {
		pauseScreen.SetActive(false);

		tutorialTrans.GetComponent<PressEventButton>().pressed += IncrementIndex;
	}

	private void Start() {
		p1turn = true;

		p1bell = game.player1.turnEndButton.GetComponentInChildren<SkinnedMeshRenderer>();
		p2bell = game.player2.turnEndButton.GetComponentInChildren<SkinnedMeshRenderer>();

		defaultBellCol = p1bell.material.color;

		if (p1turn) {
			p1mouse.disabled = false;
			p2mouse.disabled = true;

			game.player2.turnEndButton.enabled = false;
			p2bell.material.color = Color.grey;
		}
		else {
			p1mouse.disabled = true;
			p2mouse.disabled = false;

			game.player1.turnEndButton.enabled = false;
			p1bell.material.color = Color.grey;
		}

		game.turnEnded += TurnEndPlayerChange;

		StartCoroutine(DelayedStart(2f));
	}

	IEnumerator DelayedStart(float delay) {
		yield return new WaitForSeconds(delay);
		//game.LocalGameStart(p1turn);
		game.player1.Init(game.startingMana, game.cardsPerTurn);
		game.player2.Init(game.startingMana, game.cardsPerTurn);

		game.StartDrawCards(true, false);

		yield return new WaitForSeconds((game.startingHandSize + 2) * 0.25f);
		if (p1turn) {
			//game.player1.deck.FirstDraw(CheckIfClient(game.player1, true));
			game.player1.deck.FirstDraw(true);
			p2mouse.ActivateEssentials();
		}
		else {
			game.player2.deck.FirstDraw(true);
			p1mouse.ActivateEssentials();
		}

		if (sections.Count > 0)
			UpdateSection();
	}

	private void Update() {
		if (Input.GetKeyDown(pauseButton)) {
			pauseScreen.SetActive(!pauseScreen.activeInHierarchy);

			p1mouse.SetDisabled(pauseScreen.activeInHierarchy);
			p1mouse.GetComponent<KeypressCamController>().IgnoreInput(pauseScreen.activeInHierarchy);
		}

		if (CheckCurrentSegment()) {
			IncrementIndex();
		}
	}

	int lastPlayedIndex = -1;
	private bool CheckCurrentSegment() {
		if (currentSection < sections.Count) {
			TutorialSection temp = sections[currentSection];
			//perform standard checks
			switch (temp.stepTest) {
				//case TutorialSection.NextStepTest.CLICKTOPROCEED:
				default:
					return false;

				case TutorialSection.NextStepTest.DETECTCARDPLACEMENT:
					if (lastPlayedIndex != game.player1.lastPlayedIndex) {
						lastPlayedIndex = game.player1.lastPlayedIndex;

						return (temp.stepTestIndex < 0) || (temp.stepTestIndex == lastPlayedIndex);
					}
					return false;

				case TutorialSection.NextStepTest.DETECTSACRIFICEPLACEMENT:
					if (lastPlayedIndex != game.player1.lastPlayedIndex) {
						lastPlayedIndex = game.player1.lastPlayedIndex;

						if (game.player1.backLine[lastPlayedIndex].holding &&
							game.player1.backLine[lastPlayedIndex].holding.data.cost > 0)
						{
							return (temp.stepTestIndex < 0) || (temp.stepTestIndex == lastPlayedIndex);
						}
					}
					return false;
			}
		}
		//final check, can do something different
		else {
			return false;
		}
	}

	public void IncrementIndex() {
		if (++currentSection < sections.Count) {
			//update the text
			UpdateSection();

			if (currentSection == sections.Count - 1)
				StartCoroutine(DelayedFunc(delegate { lol.enabled = true; }, 30f));
		}
		else {
			//too big, can trigger something special
			Client.ExitGame();
		}
	}

	private void UpdateSection() {
		TutorialSection temp = sections[currentSection];
		if (temp.hoverOver)
			tutorialTrans.position = temp.hoverOver.position + temp.offset;
		else
			tutorialTrans.position = game.transform.position + temp.offset;

		tutorialQuad.localScale = temp.scale + Vector3.right * 0.05f + Vector3.up * 0.05f;
		tutorialTrans.GetComponent<BoxCollider>().size = temp.scale;
		tutorialTrans.GetComponent<LookAt>().UpdateCam();
		tutorialTrans.GetComponent<PressEventButton>().enabled = temp.stepTest == TutorialSection.NextStepTest.CLICKTOPROCEED;

		tutorialText.text = temp.text;
		tutorialText.rectTransform.sizeDelta = temp.scale * 10f;

		game.player1.turnEndButton.enabled = temp.canEndTurn;
		game.player1.hand.input.cantPlaceCards = !temp.canPlaceCards;

		lastPlayedIndex = game.player1.lastPlayedIndex;

		if (temp.mirrorPlayer)
			temp.aiIndex = lastPlayedIndex;

		if (temp.avoidPlayer) {
			do {
				temp.aiIndex = Random.Range(0, game.player1.backLine.Count);
			} while (temp.aiIndex == lastPlayedIndex);
		}

		if (temp.mirrorPrevious) {
			temp.stepTestIndex = lastPlayedIndex;
			lastPlayedIndex = -1;
			game.player1.lastPlayedIndex = -1;
		}

		if (temp.stepTest == TutorialSection.NextStepTest.DELAY) {
			StartCoroutine(DelayedFunc(IncrementIndex, temp.stepTestIndex));
		}

		//do ai things
		switch (temp.aiAction) {
			default:	return;

			case TutorialSection.OpponentAction.ENDTURN:
				StartCoroutine(DelayedFunc(game.player2.turnEndButton.Press, 1f));
				return;

			case TutorialSection.OpponentAction.PLACECARD:
				int index = temp.aiIndex;

				if (index < 0)
					index = Random.Range(0, game.player2.backLine.Count);

				int attempts = 10;
				while (!game.player2.backLine[index].PutCard(game.player2.heldCards[0])) {
					//if too many just abort?
					if (--attempts < 0)
						break;

					index = Random.Range(0, game.player2.backLine.Count);
				}
				return;
		}
	}

	IEnumerator DelayedFunc(System.Action func, float delayAmt) {
		yield return new WaitForSeconds(delayAmt);
		func?.Invoke();
	}
	
	void TurnEndPlayerChange() {
		//check current player, then toggle them
		if (p1turn) {
			p1turn = false;
			p1mouse.DeActivateAll();
			p1mouse.ActivateEssentials();
			
			game.player1.turnEndButton.enabled = false;
			p1bell.material.color = Color.grey;

			//delay this if it's the player's turn end, makes sure the other doesnt double press
			p2mouse.DeActivateEssentials();
			game.player2.deck.FirstDraw(true);

			game.player2.turnEndButton.enabled = true;
			p2bell.material.color = defaultBellCol;
		}
		else {
			p1turn = true;
			p2mouse.DeActivateAll();
			p2mouse.ActivateEssentials();
			game.player2.turnEndButton.enabled = false;
			p2bell.material.color = Color.grey;

			//delay this if it's the player's turn end, makes sure the other doesnt double press
			p1mouse.DeActivateEssentials();
			game.player1.deck.FirstDraw(true);

			game.player1.turnEndButton.enabled = true;
			p1bell.material.color = defaultBellCol;
		}

		if (sections[currentSection].stepTest == TutorialSection.NextStepTest.CLICKBELL)
			//sections[currentSection].aiAction == TutorialSection.OpponentAction.ENDTURN)
			IncrementIndex();
	}
}
