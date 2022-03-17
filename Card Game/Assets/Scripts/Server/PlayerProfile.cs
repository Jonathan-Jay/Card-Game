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

    public override void SetData(string username, string status) {
		this.username.text = username;
		this.status.text = status;

		if (status.Length > 8 && status.Substring(0, 8) == "In Lobby") {
			bg.color = lobbyCol;
		}
	}
}
