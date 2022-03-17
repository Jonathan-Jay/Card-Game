using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseOnDelete : MonoBehaviour
{
	//make it persist so this triggers when the enire game closes
    private void Awake() {
		DontDestroyOnLoad(gameObject);
	}

	private void OnDestroy() {
		//close the socket
		Client.Close();
	}
}
