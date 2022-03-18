using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAboveTable : MonoBehaviour
{
	[SerializeField]	Transform spawnPos;
	
	private void OnCollisionEnter(Collision other) {
		other.transform.position = spawnPos.position;
	}
}
