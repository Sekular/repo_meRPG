using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float keyPanSpeed = 7.5f;
	public float mousePanSpeed = 20f;
	public float zoomSpeed = 15f;
	public float zoomStep = 100f;
	public float rotateSpeed = 1f;


	public float dragSpeed = 2;
	private Vector3 dragOrigin;

	public bool cameraDragging = false;

	public float outerLeft = -10f;
	public float outerRight = 10f;

	void LateUpdate() {
		RunMouseInputs();
		if (!cameraDragging) {
			RunKeyboardInputs();
		}
	}

	void RunMouseInputs() {
		if (Input.GetMouseButtonDown(1)) {
			dragOrigin = Input.mousePosition;
		}

		if (Input.GetMouseButton(1)) {
			cameraDragging = true;
		}
		else {
			cameraDragging = false;
			return;
		}

		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
		Vector3.Normalize(pos);
		
		if (pos.x > 0f) {
			transform.Translate(Vector3.right * Time.deltaTime * mousePanSpeed * pos.x);
		}
		else {
			transform.Translate(Vector3.left * Time.deltaTime * mousePanSpeed * -pos.x);
		}

		if (pos.y > 0f) {
			Vector3 up = transform.TransformDirection(Vector3.forward);
			up.y = 0;
			transform.Translate(transform.InverseTransformDirection(Vector3.Normalize(up)) * Time.deltaTime * mousePanSpeed * pos.y);
		}
		else {
			Vector3 down = transform.TransformDirection(Vector3.back);
			down.y = 0;
			transform.Translate(transform.InverseTransformDirection(Vector3.Normalize(down)) * Time.deltaTime * mousePanSpeed * -pos.y);
		}
	}


	void RunKeyboardInputs() {
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
			transform.Translate(Vector3.left * Time.deltaTime * keyPanSpeed);
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
			transform.Translate(Vector3.right * Time.deltaTime * keyPanSpeed);
		}
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
			Vector3 up = transform.TransformDirection(Vector3.forward);
			up.y = 0;
			transform.Translate(transform.InverseTransformDirection(Vector3.Normalize(up)) * Time.deltaTime * keyPanSpeed);
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
			Vector3 down = transform.TransformDirection(Vector3.back);
			down.y = 0;
			transform.Translate(transform.InverseTransformDirection(Vector3.Normalize(down)) * Time.deltaTime * keyPanSpeed);
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
