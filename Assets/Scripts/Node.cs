using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A container at a grid position that holds information on neighboring grid positions.
public class Node {
	
	public int x, z;	// The nodes x, z locations. Set at runtime when the grid is generated.
	public NodeVisual m_moveVisual;     // The visual that is displayed to the player when an Actor is selecting their movement location.
	[HideInInspector] public bool m_bIsHighlighted;		// Is this node currently being highlighted for movement?
	[HideInInspector] public float m_fMovesRemaining = 0f;		// Container. Used when calculating possible Actor movement.
	[HideInInspector] public List<Node> neighbours = new List<Node>();		// References to all Nodes neighboring this one.
	[HideInInspector] public int covNorth = 0, covEast = 0, covSouth = 0, covWest = 0;		// Info on the level and direction of cover this node provides to Actors on it.
}