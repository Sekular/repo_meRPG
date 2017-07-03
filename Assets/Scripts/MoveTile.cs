using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTile : MonoBehaviour
{
    [HideInInspector] public Vector3 gridPos;
    private float offset = 0.5f;

	[HideInInspector] public int tileX, tileY;
	[HideInInspector] public Grid grid;

	void Start () {
        gridPos = transform.position;
        gridPos.y += offset;
	}

	// This is currently drawing a line on click in conjunction with Actor.Update(). Use this again later for showing the movement of characters.
	/*void OnMouseUp() {
		grid.GeneratePathTo(tileX, tileY);
	}*/
}
