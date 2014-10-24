using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	void Start () {

	}
	public float speed = 6.0F;

	private Vector3 moveDirection = Vector3.zero;

	void Update () {

		CharacterController controller = GetComponent<CharacterController>();
		if (controller.isGrounded) {
			moveDirection = new Vector3(0, 0, 1);
			
			moveDirection *= speed;

		}

		controller.Move(moveDirection * Time.deltaTime);

	}


}
