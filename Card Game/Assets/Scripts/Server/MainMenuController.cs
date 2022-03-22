using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
	[SerializeField] Client client;

	[SerializeField] TMPro.TMP_Text ipError;
	[SerializeField] TMPro.TMP_InputField ipInput;
	[SerializeField] GameObject localGameButtons;
	[SerializeField] GameObject joinOnlineButton;
	[SerializeField] UnityEngine.UI.Button joinServerButton;
	[SerializeField] UnityEngine.UI.Button leaveServerButton;
	[SerializeField] TMPro.TMP_Text lobbyName;
	[SerializeField] TMPro.TMP_Text lobbyError;
	[SerializeField] CameraController cam;
	[SerializeField] UITemplateList lobbyList;
	[SerializeField] UITemplateList playerList;
	[SerializeField] UITemplateList inLobbyPlayerList;

	private void OnEnable() {
		client.connectedEvent += EnableUI;
		client.connectingEvent += ipTextControl;
		client.leaveServerEvent += Leave;
		client.updatePlayerList += UpdatePlayerList;
		client.updateLobbyList += UpdateLobbyList;
		client.dirty += CleanLists;
		client.lobbyError += LobbyError;
		client.joinedLobby += JoinedLobby;
	}

	private void OnDisable() {
		client.connectedEvent -= EnableUI;
		client.connectingEvent -= ipTextControl;
		client.leaveServerEvent -= Leave;
		client.updatePlayerList -= UpdatePlayerList;
		client.updateLobbyList -= UpdateLobbyList;
		client.dirty -= CleanLists;
		client.lobbyError -= LobbyError;
		client.joinedLobby -= JoinedLobby;
	}

	void EnableUI(bool connected) {
		//things to hide?
		localGameButtons.SetActive(!connected);

		//things to allow
		joinServerButton.gameObject.SetActive(!connected);
		leaveServerButton.gameObject.SetActive(connected);

		joinOnlineButton.SetActive(connected);

		//only works if not working
		ipInput.interactable = !connected;

		if (connected) {
			//safety
			joinServerButton.interactable = true;

			ipInput.text = Client.server.Address.ToString();
		}
	}

	void Leave() {
		EnableUI(false);
		ipError.text = "Left";
	}

	public void TryConnect() {
		//dont allow empty
		if (ipInput.text == "") return;
		GetComponent<AudioQueue>().Play();
		client.TryConnect(ipInput.text);
	}

	void ipTextControl(bool functioning, string message) {
		ipError.text = message;

		//if functioning is true, dont let these work
		ipInput.interactable = !functioning;
		joinServerButton.interactable = !functioning;

		//if not functioning, reset ipInput;
		if (!functioning)
			ipInput.text = "";
	}

	void UpdatePlayerList(bool inLobby, string message) {
		if (inLobby) {
			inLobbyPlayerList.CreateProfile(message);
		}
		else
			playerList.CreateProfile(message);
	}

	void UpdateLobbyList(string message) {
		lobbyList.CreateProfile(message);
	}

	void CleanLists() {
		//jsut deletes everything it doesn't want
		inLobbyPlayerList.Clear();
		playerList.Clear();
		lobbyList.Clear();
	}

	void LobbyError(string message) {
		lobbyError.text = message;
	}

	void JoinedLobby(bool joined, string lobbyName) {
		this.lobbyName.text = lobbyName;
		if (joined) {
			while (cam.index != 2) {
				cam.IncrementIndex(true);
			}
		}
		else {
			while (cam.index != 1) {
				cam.IncrementIndex(true);
			}
		}
	}
}
