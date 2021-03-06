using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
	static bool firstScreen = true;
	[SerializeField] Client client;

	[SerializeField] CameraController cam;
	[SerializeField] TMP_Text ipError;
	[SerializeField] TMP_InputField ipInput;
	[SerializeField] GameObject localGameButtons;
	[SerializeField] GameObject onlineGameButton;
	[SerializeField] Button joinServerButton;
	[SerializeField] Button leaveServerButton;
	[SerializeField] UITemplateList leaderboardList;
	[SerializeField] int outLobbyIndex = 2;
	[SerializeField] TMP_Text lobbyError;
	[SerializeField] UITemplateList lobbyList;
	[SerializeField] UITemplateList playerList;
	[SerializeField] int inLobbyIndex = 3;
	[SerializeField] TMP_Text lobbyName;
	[SerializeField] UITemplateList inLobbyPlayerList;
	[SerializeField] GameObject joinPlayer1Seat;
	[SerializeField] GameObject player1Seat;
	[SerializeField] GameObject joinPlayer2Seat;
	[SerializeField] GameObject player2Seat;
	[SerializeField] Button leaveSeat;
	[SerializeField] Button startButton;
	[SerializeField] AudioQueue goForwardPlayer;
	[SerializeField] AudioQueue goBackPlayer;
	[SerializeField] AudioQueue connectedAudioPlayer;
	[SerializeField] AudioQueue seatedAudioPlayer;

	private void OnEnable() {
		client.connectedEvent += EnableUI;
		client.connectingEvent += ipTextControl;
		client.leaveServerEvent += Leave;
		client.updatePlayerList += UpdatePlayerList;
		client.updateLobbyList += UpdateLobbyList;
		client.updateLeaderboardList += UpdateLeaderboardList;
		client.dirty += CleanLists;
		client.lobbyError += LobbyError;
		client.joinedLobby += JoinedLobby;
		client.tableSeatUpdated += SeatedPlayersChanged;

		//make the start game button not work right away?
	}

	private void OnDisable() {
		client.connectedEvent -= EnableUI;
		client.connectingEvent -= ipTextControl;
		client.leaveServerEvent -= Leave;
		client.updatePlayerList -= UpdatePlayerList;
		client.updateLobbyList -= UpdateLobbyList;
		client.updateLeaderboardList -= UpdateLeaderboardList;
		client.dirty -= CleanLists;
		client.lobbyError -= LobbyError;
		client.joinedLobby -= JoinedLobby;
		client.tableSeatUpdated -= SeatedPlayersChanged;
	}

	private void Start() {
		if (firstScreen) {
			firstScreen = false;
			cam.DecrementIndex(false);
		}
	}

	void EnableUI(bool connected) {
		//things to hide?
		MultiButtonInteractable(localGameButtons, !connected);

		//things to allow
		joinServerButton.gameObject.SetActive(!connected);
		leaveServerButton.gameObject.SetActive(connected);

		MultiButtonInteractable(onlineGameButton, connected);

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
		//disable these
		MultiButtonInteractable(localGameButtons, false);

		connectedAudioPlayer?.Play();
		client.TryConnect(ipInput.text);
	}

	void ipTextControl(bool functioning, string message) {
		ipError.text = message;

		//if functioning is true, dont let these work
		ipInput.interactable = !functioning;
		joinServerButton.interactable = !functioning;

		//things to hide?
		MultiButtonInteractable(localGameButtons, !functioning);

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

	void UpdateLeaderboardList(string message) {
		leaderboardList.CreateProfile(message);
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
			if (cam.index != 1)
				goForwardPlayer?.PlayRandom();

			while (cam.index != inLobbyIndex) {
				cam.IncrementIndex(true);
			}
		}
		else {
			if (cam.index != 1)
				goBackPlayer?.PlayRandom();

			while (cam.index != outLobbyIndex) {
				cam.IncrementIndex(true);
			}
		}
	}

	int prevSeatedCount = 0;
	void SeatedPlayersChanged() {
		if (ServerManager.CheckIfClient(null, false)) {
			//show the leave seat if you're a player
			leaveSeat.interactable = true;
			joinPlayer1Seat.SetActive(false);
			joinPlayer2Seat.SetActive(false);
			
			startButton.interactable = (ServerManager.p1Index >= 0 && ServerManager.p2Index >= 0);
		}
		else {
			leaveSeat.interactable = false;
			//make button work if no player
			joinPlayer1Seat.SetActive(ServerManager.p1Index < 0);
			joinPlayer2Seat.SetActive(ServerManager.p2Index < 0);

			startButton.interactable = false;
		}

		int seatedCount = 0;
		if (ServerManager.p1Index >= 0)	++seatedCount;
		if (ServerManager.p2Index >= 0)	++seatedCount;

		if (seatedCount != prevSeatedCount) {
			if (seatedCount > 0 || prevSeatedCount > 0)
				seatedAudioPlayer?.PlayRandom();
			prevSeatedCount = seatedCount;
		}

		//render faces if valid or smt
		player1Seat.SetActive(ServerManager.p1Index >= 0);
		player2Seat.SetActive(ServerManager.p2Index >= 0);
	}

	void MultiButtonInteractable(GameObject parent, bool value) {
		foreach (Button button in parent.GetComponentsInChildren<Button>()) {
			button.interactable = value;
		}
	}
}
