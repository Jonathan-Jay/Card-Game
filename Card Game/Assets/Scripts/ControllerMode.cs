using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerMode : MonoBehaviour
{
	public ControllerHand hand;
	public Mouse mouse;
	public CameraController cam;
	public KeypressCamController camController;
	public PreviewCard previewer;
	public ServerManager manager;
	public InputAction previewCard;
	public InputAction moveCursor;
	public InputAction tryInteract;
	public InputAction tryCancel;
	public float delay = 0.2f;
	public Vector3 offset = Vector3.up;

	int lastIndex;
	CardHolder[][] field;
	Vector2Int input;
	Vector2Int fieldPosition;
	bool moving = false;
	bool previewing = false;
	Card grabbed;

	private void Awake() {
		moveCursor.started += ctx => StartCoroutine(ConstantMove(ctx.ReadValue<Vector2>()));
		moveCursor.performed += ctx => {
			Vector2 val = ctx.ReadValue<Vector2>();
			input = new Vector2Int(Mathf.RoundToInt(val.x), Mathf.RoundToInt(val.y));
		};
		moveCursor.canceled += ctx => moving = false;

		previewCard.started += ctx => {
			previewer.PreviewCardData(fieldPosition.x >= 2, field[fieldPosition.y][fieldPosition.x].holding);
			previewing = true;
		};
		previewCard.canceled += ctx => {
			previewer.PreviewCardData(false, null);
			previewing = false;
		};

		//for placing card
		tryInteract.started += ctx => mouse.ForwardClickEvent(field[fieldPosition.y][fieldPosition.x].transform);
		tryInteract.canceled += ctx => {
			mouse.ForwardReleaseEvent(field[fieldPosition.y][fieldPosition.x].transform);
			//if no more steps
			if (mouse.activeSpells == 0) {
				tryInteract.Disable();
				tryCancel.Disable();
				mouse.player.turnEndButton.enabled = true;
				manager?.swapCam.Enable();
				cam.DecrementIndex(false);
				camController.enabled = true;
				hand.enabled = true;
			}
			//if it kinda fails, just make the player manually leave
		};

		tryCancel.started += ctx => {
			//are we holding thing? if so, release card in hand
			if (mouse.holding)
				mouse.ForwardReleaseEvent(hand.transform);

			//are we in spell/sacrifice mode? if so, cancel it
			if (mouse.activeSpells > 0 && grabbed) {
				if (grabbed.placement)
					mouse.ForwardClickEvent(grabbed.placement.transform);
				grabbed = null;
			}

			tryInteract.Disable();
			tryCancel.Disable();
			mouse.player.turnEndButton.enabled = true;
			manager?.swapCam.Enable();
			cam.DecrementIndex(false);
			camController.enabled = true;
			hand.enabled = true;
		};
	}

	private void Start() {
		hand.interact.started += ctx => {
			if (mouse.holding) {
				cam.IncrementIndex(false);
				cam.IncrementIndex(false);
				camController.enabled = false;
				hand.enabled = false;

				tryInteract.Enable();
				tryCancel.Enable();
				mouse.player.turnEndButton.enabled = false;
				manager?.swapCam.Disable();
				grabbed = mouse.holding.GetComponent<Card>();
			}
		};
		lastIndex = cam.index;

		//generate the field
		field = new CardHolder[4][];

		PlayerData current = mouse.player;
		PlayerData opposing = mouse.player.field[0].opposingData;

		field[0] = current.backLine.ToArray();
		field[1] = current.field.ToArray();
		field[2] = opposing.field.ToArray();
		field[3] = opposing.backLine.ToArray();

		//if p2, flip x axis
		if (ServerManager.game && mouse.player == ServerManager.game.player2) {
			moveCursor.ApplyBindingOverride(new InputBinding {overrideProcessors = "InvertVector2(invertX=true,invertY=false)" });
			fieldPosition = Vector2Int.up + Vector2Int.right * 3;
		}
		else {
			fieldPosition = Vector2Int.up;
		}
	}

	private void Update() {
		if (cam.index != lastIndex) {
			lastIndex = cam.index;

			hand.enabled = lastIndex < 2;

			if (lastIndex == 2) {
				previewCard.Enable();
				moveCursor.Enable();
				mouse.mouseObject.position = field[fieldPosition.y][fieldPosition.x].transform.position + offset;
				if (tryInteract.enabled) {
					mouse.ForwardHoverEvent(field[fieldPosition.y][fieldPosition.x].transform);
				}
			}
			else if (lastIndex == 1) {
				previewCard.Disable();
				moveCursor.Disable();
				moving = false;
				if (previewing) {
					previewer.PreviewCardData(false, null);
					previewing = false;
				}
			}

			if (!tryInteract.enabled) {
				mouse.targettingCursor.gameObject.SetActive(lastIndex == 2);
				mouse.defaultCursor.gameObject.SetActive(lastIndex != 2);
			}
		}
	}

	IEnumerator ConstantMove(Vector2 val) {
		input = new Vector2Int(Mathf.RoundToInt(val.x), Mathf.RoundToInt(val.y));
		Vector2Int prev = input;
		moving = true;
		float counter = 0;

		while (moving) {
			if (mouse.disabled) {
				yield return null;
				continue;
			}

			if (counter > 0f) {
				counter -= Time.deltaTime;
				if (counter < 0f)	counter = 0f;
			}

			if (counter <= 0f || prev != input) {
				fieldPosition.y = Mathf.Clamp(fieldPosition.y + input.y, 0, field.Length - 1);
				fieldPosition.x = Mathf.Clamp(fieldPosition.x + input.x, 0, field[fieldPosition.y].Length - 1);

				mouse.mouseObject.position = field[fieldPosition.y][fieldPosition.x].transform.position + offset;
				if (tryInteract.enabled) {
					mouse.ForwardHoverEvent(field[fieldPosition.y][fieldPosition.x].transform);
				}

				if (previewing) {
					previewer.PreviewCardData(fieldPosition.x >= 2, field[fieldPosition.y][fieldPosition.x].holding);
				}

				prev = input;
				counter = delay;
			}
			yield return null;
		}
	}
}
