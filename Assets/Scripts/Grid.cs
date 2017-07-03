using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour {
	public GameObject selectedUnit;

	public int mapSizeX, mapSizeY; // The size of the map to create.

	int[,] tiles; // Array of tile data. Non-visual.
	// TODO TileType change from an Array to an Enum.
	public TileType[] tileTypes; // Array of TileTypes that information on how the tile effects movement and what visual to display for the respective tile. Visual.

	Node[,] graph; // Array of Nodes that contain references to their neighbours. Non-visual.
	public NodeVisual moveVisual;

	// Converts tile values into world position.
	public Vector3 TileToWorld(int x, int y) { return new Vector3(x, 0, y); }
	// Checks if an Actor can enter a tile based on the TileType v MovementType
	public bool ValidTile(int x, int y) { return tileTypes[tiles[x, y]].isWalkable; }

	void Start() {
		// Set selected Actors' variables.
		selectedUnit.GetComponent<Actor>().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<Actor>().tileY = (int)selectedUnit.transform.position.z;
		selectedUnit.GetComponent<Actor>().grid = this;

		GenerateMapData();
		GenerateGraph();
		GenerateGrid();
	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.Space)) {
			DisplayAvailableMovement(selectedUnit.GetComponent<Actor>().moveSpeed);
		}
	}

	// Initializes the basic map data.
	void GenerateMapData() {
		// Allocate map tiles
		tiles = new int[mapSizeX, mapSizeY];

		// Initialize map tiles
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				tiles[x, y] = 0;
			}
		}

		// Difficult Area
		for (int x = 3; x <= 5; x++) {
			for (int y = 0; y < 4; y++) {
				tiles[x, y] = 1;
			}
		}

		// Impassable area
		tiles[4, 4] = 2;
		tiles[5, 4] = 2;
		tiles[6, 4] = 2;
		tiles[7, 4] = 2;
		tiles[8, 4] = 2;

		tiles[4, 5] = 2;
		tiles[4, 6] = 2;
		tiles[8, 5] = 2;
		tiles[8, 6] = 2;
	}

	// Returns the cost to enter the tile coordinates parsed.
	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY) {
		TileType tt = tileTypes[ tiles[targetX, targetY] ];

		if(!ValidTile(targetX, targetY)) {
			return Mathf.Infinity;
		}

		float cost = tt.moveCost;

		// Detect diagonal movement
		if (sourceX != targetX && sourceY != targetY) {
			cost += 0.01f;
		}

		return cost;
	}

	// Generates the grid graph for path-finding. Creates all Nodes in the grid.
	void GenerateGraph() {
		// Initialize the array.
		graph = new Node[mapSizeX, mapSizeY];

		// Initialize a Node for each array location.
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				graph[x, y] = new Node();

				graph[x, y].x = x;
				graph[x, y].y = y;

				graph[x, y].visual = (NodeVisual)Instantiate(moveVisual, new Vector3(x, 0.05f, y), Quaternion.identity);
				graph[x, y].visual.Deactivate();
			}
		}

		// Calculate all neighbours (Grid must be constructed first).
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				// For 8-way movement.
				// Find 3 left neighbours
				if (x > 0) {
					graph[x, y].neighbours.Add(graph[x - 1, y]);
					if (y > 0) { graph[x, y].neighbours.Add(graph[x - 1, y - 1]); }
					if (y < mapSizeY - 1) { graph[x, y].neighbours.Add(graph[x - 1, y + 1]); }
				}

				// Find 3 right neighbours
				if (x < mapSizeX - 1) {
					graph[x, y].neighbours.Add(graph[x + 1, y]);
					if (y > 0) { graph[x, y].neighbours.Add(graph[x + 1, y - 1]); }
					if (y < mapSizeY - 1) { graph[x, y].neighbours.Add(graph[x + 1, y + 1]); }
				}

				// Find up and down.
				if (y > 0) { graph[x, y].neighbours.Add(graph[x, y - 1]); }
				if (y < mapSizeY - 1){ graph[x, y].neighbours.Add(graph[x, y + 1]);	}
			}
		}
	}

	// Generates the physical movement grid and initializes MoveTiles.
	void GenerateGrid() {
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				TileType t = tileTypes[tiles[x, y]];

				GameObject go = Instantiate(t.tileObject, new Vector3(x, 0f, y), Quaternion.identity);

				MoveTile mt = go.GetComponent<MoveTile>();
				mt.tileX = x;
				mt.tileY = y;
				mt.grid = this;
			}
		}
	}

	// Used for calculating path-finding. Constructs all Grid Nodes. 
	public void GeneratePathTo(int x, int y) {
		// Clear old path.
		selectedUnit.GetComponent<Actor>().currentPath = null;

		// Return if the tile is not valid (eg: impassable)
		if(!ValidTile(x,y)) { return; }

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		// Contains the chain of Nodes from source to destination.
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// List out Nodes that hasn't yet been checked.
		List<Node> unvisited = new List<Node>();

		// The source node for path-finding purposes.
		Node source = graph[selectedUnit.GetComponent<Actor>().tileX, selectedUnit.GetComponent<Actor>().tileY];

		// The target node for path-finding purposes.
		Node target = graph[x,y];

		dist[source] = 0;
		prev[source] = null;

		// Initialize to infinite distance.
		foreach(Node v in graph) {
			if(v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}
			unvisited.Add(v);
		}

		// The main loop.
		while(unvisited.Count > 0) {
			Node u = null;

			// u will be the unvisited node with the smallest distance.
			foreach (Node possibleU in unvisited) {
				if(u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			// If shortest distance is the target, exit loop.
			if(u == target) { break; }

			// Mark Node as visited, remove from unvisited list.
			unvisited.Remove(u);

			foreach(Node v in u.neighbours) {
				// Checks distances between current node and neighboring nodes. and updates prev nodes if their distances is shorter than the one already logged.
				// float alt = dist[u] + u.DistanceTo(v);
				float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
				if (alt < dist[v]) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}
		// Have found the shortest route OR have not found a route.
		if(prev[target] == null) {
			Debug.Log("No route to target location.");
			return;
		}

		List<Node> currentPath = new List<Node>();
		Node curr = target;

		// Step through the "prev" chain and add it to path.
		while(curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}
		// Current path is target to source, it must be inverted.
		currentPath.Reverse();

		selectedUnit.GetComponent<Actor>().currentPath = currentPath;
	}

	public void DisplayAvailableMovement(float startingMovement) {
		ResetMovement();

		// Create a list of nodes to check for movement and add this starting node (where the character currently is).
		List<Node> nodesToCheck = new List<Node>();
		List<Node> nodesToHighlight = new List<Node>();
		nodesToCheck.Add(graph[selectedUnit.GetComponent<Actor>().tileX, selectedUnit.GetComponent<Actor>().tileY]);
		nodesToHighlight.Add(graph[selectedUnit.GetComponent<Actor>().tileX, selectedUnit.GetComponent<Actor>().tileY]);

		graph[selectedUnit.GetComponent<Actor>().tileX, selectedUnit.GetComponent<Actor>().tileY].isHighlighted = true;

		graph[selectedUnit.GetComponent<Actor>().tileX, selectedUnit.GetComponent<Actor>().tileY].movesRemaining = startingMovement;

		// Find all connected nodes with available movement in range.
		while (nodesToCheck.Count > 0) {
			Node n = nodesToCheck[0];

			foreach (Node neighbour in n.neighbours) {
				if (n.movesRemaining - CostToEnterTile(n.x, n.y, neighbour.x, neighbour.y) > 0f) {
					if ((n.movesRemaining - CostToEnterTile(n.x, n.y, neighbour.x, neighbour.y)) > neighbour.movesRemaining) {
						nodesToCheck.Add(neighbour);
						neighbour.movesRemaining = n.movesRemaining - CostToEnterTile(n.x, n.y, neighbour.x, neighbour.y);
						if (!neighbour.isHighlighted) {
							nodesToHighlight.Add(neighbour);
						}
					}
				}
			}

			nodesToCheck.Remove(n);
		}
		
		foreach (Node node in nodesToHighlight) {
			node.visual.Activate();
		}
	}

	public void ResetMovement() {
		foreach(Node node in graph) {
			node.movesRemaining = 0;
			node.isHighlighted = false;
			node.visual.Deactivate();
		}
	}
}
