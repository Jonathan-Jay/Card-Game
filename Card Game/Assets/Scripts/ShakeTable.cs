using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTable : MonoBehaviour
{
	public float range = 15f;
	public float counter = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) {
			if (Input.GetKeyDown(KeyCode.Space))
				counter = range;
			transform.rotation = Quaternion.Euler(Random.Range(-counter, counter), Random.Range(-counter, counter), Random.Range(-range, range));
			counter += Time.deltaTime;
		}
		else if (transform.rotation != Quaternion.identity) {
			transform.rotation = Quaternion.identity;
		}
    }
}
