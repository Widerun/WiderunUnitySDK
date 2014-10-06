using UnityEngine;
using System.Collections;

public class GetSlope : MonoBehaviour {


	void OnControllerColliderHit(ControllerColliderHit hit) {
		float angle = Vector3.Angle (hit.normal, Vector3.up);
		angle = angle*(Vector3.Angle (hit.normal, Vector3.forward)<90?-1:1);

		Debug.Log (angle);
	}
}
