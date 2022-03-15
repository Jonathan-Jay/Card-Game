using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayAfloat : MonoBehaviour
{

    //force the thing to stay upright
    void LateUpdate()
    {
		if (transform.rotation != Quaternion.identity) {
			transform.rotation = Quaternion.identity;
		}
    }
}
