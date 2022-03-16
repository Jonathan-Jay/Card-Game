using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextChat : MonoBehaviour
{
    public TMPro.TMP_Text chatBox;
    public void UpdateChat(string message)
    {
        chatBox.text += message + "\n";
    }
}
