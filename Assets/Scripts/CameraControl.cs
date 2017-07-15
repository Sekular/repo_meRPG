using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float panSpeed = 7.5f;
	public float zoomSpeed = 15f;
	public float zoomStep = 100f;
	public float rotateSpeed = 1f;

	void Update() {
		RunInputs();
	}

	void RunInputs() {
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
			transform.Translate(Vector3.left * Time.deltaTime * panSpeed);
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
			transform.Translate(Vector3.right * Time.deltaTime * panSpeed);
		}
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
			Vector3 up = transform.TransformDirection(Vector3.forward);
			up.y = 0;
			transform.Translate(transform.InverseTransformDirection(Vector3.Normalize(up)) * Time.deltaTime * panSpeed);
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
			Vector3 down = transform.TransformDirection(Vector3.back);
			down.y = 0;
			transform.Translate(transform.InverseTransformDirection(Vector3.Normalize(down)) * Time.deltaTime * panSpeed);
		}
		if (Input.GetKey(KeyCode.KeypadPlus)) {
			transform.Translate(Vector3.forward * Time.deltaTime * zoomSpeed);
		}
		if (Input.GetKey(KeyCode.KeypadMinus)) {
			transform.Translate(Vector3.back * Time.deltaTime * zoomSpeed);
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0) {
			transform.Translate(Vector3.forward * Time.deltaTime * zoomStep);
		}
		if (Input.GetAxis("Mouse ScrollWheel") < 0) {
			transform.Translate(Vector3.back * Time.deltaTime * zoomStep);
		}
	}
}
