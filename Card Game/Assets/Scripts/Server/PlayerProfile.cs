using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerProfile : UITemplate
{
	[SerializeField] TMP_Text username;
	[SerializeField] TMP_Text status;
	[SerializeField] UnityEngine.UI.Image bg;
	[SerializeField] Color lobbyCol = Color.blue;
	int playerId;

    public override void SetData(string username, int id, string status) {
		this.username.text = username;
		this.status.text = status;
		playerId = id;

		if (status.Length > 8 && status.Substring(0, 8) == "In Lobby") {
			bg.color = lobbyCol;
		}
	}
}
