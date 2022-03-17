using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerProfile : UITemplate
{
	[SerializeField] TMP_Text username;
	[SerializeField] TMP_Text status;

    public override void SetData(string username, string status) {
		this.username.text = username;
		this.status.text = status;
	}
}
