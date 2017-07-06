using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadIcon : MonoBehaviour {

	private Grid grid;

	void Start () {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
	}


	void OnMouseOver() {
		if (Input.GetMouseButtonDown(0)) {
			grid.selectedActor.Reload();
		}
	}
}
