using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float m_fKeyPanSpeed = 7.5f;		// Rate at which the camera pans from a keyboard input.
	public float m_fMousePanSpeed = 20f;	// Rate at which the camera pans from a mouse input.
	public float m_fZoomSpeed = 15f;		// Rate at which the camera zooms from a keyboard input.
	public float m_fZoomStep = 100f;		// Rate at which the camera zooms from a mouse-wheel input.

	[HideInInspector] public bool m_bCameraDragging = false;	// Is the camera currently being moved via the mouse?

	private Vector3 m_vDragOrigin;	// The origin point of when the player begins a mouse move input. Used to calculate drag direction.

	void LateUpdate() {
		MouseInputs();
		if (!m_bCameraDragging) { KeyboardInputs(); }
	}

	void MouseInputs() {
		if (Input.GetAxis("Mouse ScrollWheel") > 0) { MoveCamera(Vector3.forward, m_fZoomStep); }
		if (Input.GetAxis("Mouse ScrollWheel") < 0) { MoveCamera(Vector3.back, m_fZoomStep); }

		if (Input.GetMouseButtonDown(1)) { m_vDragOrigin = Input.mousePosition; }
		if (Input.GetMouseButton(1)) {
			m_bCameraDragging = true;
		}
		else {
			m_bCameraDragging = false;
			return;
		}

		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - m_vDragOrigin);
		Vector3.Normalize(pos);
		
		if (pos.x > 0f) {
			MoveCamera(Vector3.right, m_fMousePanSpeed * pos.x);
		}
		else {
			MoveCamera(Vector3.left, m_fMousePanSpeed * -pos.x);
		}

		if (pos.y > 0f) {
			Vector3 up = transform.TransformDirection(Vector3.forward);
			up.y = 0;
			MoveCamera(transform.InverseTransformDirection(Vector3.Normalize(up)), m_fMousePanSpeed * pos.y);
		}
		else {
			Vector3 down = transform.TransformDirection(Vector3.back);
			down.y = 0;
			MoveCamera(transform.InverseTransformDirection(Vector3.Normalize(down)), m_fMousePanSpeed * -pos.y);
		}
	}

	void KeyboardInputs() {
		if (Input.GetKey(KeyCode.KeypadPlus)) { MoveCamera(Vector3.forward, m_fZoomSpeed); }
		if (Input.GetKey(KeyCode.KeypadMinus)) { MoveCamera(Vector3.back, m_fZoomSpeed); }
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) { MoveCamera(Vector3.left, m_fKeyPanSpeed); }
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) { MoveCamera(Vector3.right, m_fKeyPanSpeed); }

		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
			Vector3 up = transform.TransformDirection(Vector3.forward);
			up.y = 0;
			MoveCamera(transform.InverseTransformDirection(Vector3.Normalize(up)), m_fKeyPanSpeed);
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
			Vector3 down = transform.TransformDirection(Vector3.back);
			down.y = 0;
			MoveCamera(transform.InverseTransformDirection(Vector3.Normalize(down)), m_fKeyPanSpeed);
		}
	}

	void MoveCamera(Vector3 moveDir, float moveSpeed) {
		transform.Translate(moveDir * Time.deltaTime * moveSpeed);
	}
}
