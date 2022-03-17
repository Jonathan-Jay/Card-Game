using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerProfile : UITemplate
{
	TMP_Text username;
	TMP_Text status;

    public override void SetData(string username, string status) {
		this.status.text = username;
		this.username.text = status;
	}
}
