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
    public override void SetData(string username, string playerCount) {
		//all usernames should start with the index
		index = int.Parse(username.Substring(0, 1));
		lobbyName.text = username.Substring(1);
		this.playerCount.text = playerCount + " player(s)";
	}

	public void JoinLobby() {
		Client.JoinLobby(index);
	}
}
