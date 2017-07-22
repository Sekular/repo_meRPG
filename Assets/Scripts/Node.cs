using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A container at a grid position that holds information on neighboring grid positions.
public class Node {
	public List<Node> neighbours;
	public int x, z;
	public NodeVisual visual;
	public float movesRemaining;
	public bool isHighlighted;
	[HideInInspector] public int covNorth = 0, covEast = 0, covSouth = 0, covWest = 0;

	// Constructor
	public Node() {
		neighbours = new List<Node>();
	}

	// Helper - Returns distance between two nodes
	public float DistanceTo(Node n)	{ return Vector2.Distance(new Vector2(x, z), new Vector2(n.x, n.z)); }

}