using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTable : MonoBehaviour
{
	public float range = 15f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) {
			transform.rotation = Quaternion.Euler(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
		}
		else if (transform.rotation != Quaternion.identity) {
			transform.rotation = Quaternion.identity;
		}
    }
}
