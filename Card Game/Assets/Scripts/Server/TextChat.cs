using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextChat : MonoBehaviour
{
	[SerializeField] int maxLines = 13;
	[SerializeField] TMPro.TMP_Text chatBox;
	[SerializeField] GameObject chat;
	[SerializeField] UnityEngine.UI.Toggle showBox;
	[SerializeField] Color notificationCol = Color.green;
	[SerializeField] Color defaultCol = Color.white;
	static string chatString = "";

	private void OnEnable() {
		//also retrieve string
		chatBox.text = chatString;
		Toggled();
	}

	private void OnDisable() {
		showBox.isOn = true;
	}

    public void UpdateChat(string message) {
        chatBox.text += message + "\n";
		if (chatBox.textInfo.lineCount > maxLines) {
			//only an estimate, but it works
			chatBox.text = chatBox.text.Remove(0, chatBox.text.IndexOf('\n') + 1);
		}
		//store it in the string for reloading purposes
		chatString = chatBox.text;

		//provide notification if not on
		if (!showBox.isOn) {
			showBox.image.color = notificationCol;
		}
    }

	public void Reload() {
		chatBox.text = chatString;
	}

	public void Toggled() {
		if (chat.activeInHierarchy != showBox.isOn) {
			chat.SetActive(showBox.isOn);
			
			//turn off the colour when opened
			if (showBox.isOn) {
				showBox.image.color = defaultCol;
			}
		}
	}
}
