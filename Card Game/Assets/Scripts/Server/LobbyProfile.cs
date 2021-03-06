using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyProfile : UITemplate
{
	[SerializeField] TMP_Text lobbyName;
	[SerializeField] TMP_Text playerCount;
	int index;
    public override void SetData(string username, int id, string playerCount) {
		//all usernames should start with the index
		index = id;
		lobbyName.text = username;
		this.playerCount.text = playerCount + " player(s)";
	}

	public void JoinLobby() {
		Client.JoinLobby(index);
	}
}
