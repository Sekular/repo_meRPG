using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTile : MonoBehaviour
{
    [HideInInspector] public Vector3 m_vGridPos;    // The MoveTile's position including the offset. Used for placing it in the world during grid generation.
    private float offset = 0.5f;    // Offset for placing the relevant object for the placeholder grid visual.

	public int m_iTileX, m_iTileZ;    // The MoveTile's X and Z coordinates.
	[HideInInspector] public Grid grid;   // Reference to the grid.

	void Start () {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
    m_vGridPos = transform.position;
    m_vGridPos.z += offset;
    m_iTileX = (int)transform.position.x;
    m_iTileZ = (int)transform.position.z;
	}

	// This is currently drawing a line on click in conjunction with Actor.Update().
	void OnMouseOver() {
		if (grid.selectedActor) {
			if (!grid.selectedActor.isMoving)
				grid.GeneratePathTo((int)transform.position.x, (int)transform.position.z);
			grid.selectedActor.GetComponent<Actor>().ShowPathTo();
		}
	}
}
