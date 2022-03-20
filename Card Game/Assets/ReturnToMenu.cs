using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToMenu : MonoBehaviour
{
	public KeyCode exitKey;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(exitKey)) {
			//SceneController.ChangeScene("Main Menu");
			Client.ExitGame();
		}
    }
}
