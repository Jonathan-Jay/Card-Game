using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardProfile : UITemplate
{
	[SerializeField] TMP_Text ranking;
	[SerializeField] TMP_Text score;
	[SerializeField] TMP_Text text;

    public override void SetData(string user, int ranking, string wins) {
		this.ranking.text = ranking + ".";
		this.score.text = wins;
		this.text.text = user;
	}
}
