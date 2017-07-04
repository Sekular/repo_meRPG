using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTile : MonoBehaviour
{
    [HideInInspector] public Vector3 gridPos;
    private float offset = 0.5f;

	public int tileX, tileZ;
	[HideInInspector] public Grid grid;

	void Start () {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		gridPos = transform.position;
        gridPos.z += offset;
		tileX = (int)transform.position.x;
		tileZ = (int)transform.position.z;
	}

	// This is currently drawing a line on click in conjunction with Actor.Update(). Use this again later for showing the movement of characters.
	void OnMouseOver() {
		if (grid.selectedActor) {
			if (!grid.selectedActor.isMoving)
				grid.GeneratePathTo((int)transform.position.x, (int)transform.position.z);
			grid.selectedActor.GetComponent<Actor>().ShowPathTo();
		}
	}
}
