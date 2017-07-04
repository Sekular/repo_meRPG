using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour {
	[HideInInspector] public Actor selectedActor;

	public int mapSizeX, mapSizeZ; // The size of the map to create.

	int[,] tiles; // Array of tile data. Non-visual.
	// TODO TileType change from an Array to an Enum.
	public TileType[] tileTypes; // Array of TileTypes that information on how the tile effects movement and what visual to display for the respective tile. Visual.

	Node[,] graph; // Array of Nodes that contain references to their neighbours. Non-visual.
	public NodeVisual moveVisual;

	// Converts tile values into world position.
	public Vector3 TileToWorld(int x, int z) { return new Vector3(x, 0, z); }
	// Checks if an Actor can enter a tile based on the TileType v MovementType
	public bool ValidTile(int x, int z) { return tileTypes[tiles[x, z]].isWalkable; }

	void Start() {
		GenerateMapData();
		GenerateGraph();
		GenerateGrid();
	}
	
	// Initializes the basic map data.
	void GenerateMapData() {
		// Allocate map tiles
		tiles = new int[mapSizeX, mapSizeZ];

		// Initialize map tiles
		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				tiles[x, z] = 0;
			}
		}

		// Difficult Area
		for (int x = 3; x <= 5; x++) {
			for (int z = 0; z < 4; z++) {
				tiles[x, z] = 1;
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
	public float CostToEnterTile(int sourceX, int sourceZ, int targetX, int targetZ) {
		TileType tt = tileTypes[ tiles[targetX, targetZ] ];

		if(!ValidTile(targetX, targetZ)) {
			return Mathf.Infinity;
		}

		float cost = tt.moveCost;

		// Detect diagonal movement
		if (sourceX != targetX && sourceZ != targetZ) {
			cost += 0.01f;
		}

		return cost;
	}

	// Generates the grid graph for path-finding. Creates all Nodes in the grid.
	void GenerateGraph() {
		// Initialize the array.
		graph = new Node[mapSizeX, mapSizeZ];

		// Initialize a Node for each array location.
		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				graph[x, z] = new Node();

				graph[x, z].x = x;
				graph[x, z].z = z;

				graph[x, z].visual = (NodeVisual)Instantiate(moveVisual, new Vector3(x, 0.05f, z), Quaternion.identity);
				graph[x, z].visual.Deactivate();
			}
		}

		// Calculate all neighbours (Grid must be constructed first).
		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				// For 8-way movement.
				// Find 3 left neighbours
				if (x > 0) {
					graph[x, z].neighbours.Add(graph[x - 1, z]);
					if (z > 0) { graph[x, z].neighbours.Add(graph[x - 1, z - 1]); }
					if (z < mapSizeZ - 1) { graph[x, z].neighbours.Add(graph[x - 1, z + 1]); }
				}

				// Find 3 right neighbours
				if (x < mapSizeX - 1) {
					graph[x, z].neighbours.Add(graph[x + 1, z]);
					if (z > 0) { graph[x, z].neighbours.Add(graph[x + 1, z - 1]); }
					if (z < mapSizeZ - 1) { graph[x, z].neighbours.Add(graph[x + 1, z + 1]); }
				}

				// Find up and down.
				if (z > 0) { graph[x, z].neighbours.Add(graph[x, z - 1]); }
				if (z < mapSizeZ - 1){ graph[x, z].neighbours.Add(graph[x, z + 1]);	}
			}
		}
	}

	// Generates the physical movement grid and initializes MoveTiles.
	void GenerateGrid() {
		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				TileType t = tileTypes[tiles[x, z]];

				GameObject go = Instantiate(t.tileObject, new Vector3(x, 0f, z), Quaternion.identity);

				MoveTile mt = go.GetComponent<MoveTile>();
				mt.tileX = x;
				mt.tileZ = z;
				mt.grid = this;
			}
		}
	}

	// Used for calculating path-finding. Constructs all Grid Nodes. 
	public void GeneratePathTo(int x, int z) {
		// Clear old path.
		selectedActor.GetComponent<Actor>().currentPath = null;

		// Return if the tile is not valid (eg: impassable)
		if(!ValidTile(x,z)) { return; }

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		// Contains the chain of Nodes from source to destination.
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// List out Nodes that hasn't yet been checked.
		List<Node> unvisited = new List<Node>();

		// The source node for path-finding purposes.
		Node source = graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z];

		// The target node for path-finding purposes.
		Node target = graph[x,z];

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
				float alt = dist[u] + CostToEnterTile(u.x, u.z, v.x, v.z);
				if (alt < dist[v]) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}
		// Have found the shortest route OR have not found a route.
		if(prev[target] == null) {
			//Debug.Log("No route to target location.");
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

		selectedActor.GetComponent<Actor>().currentPath = currentPath;
	}

	public void DisplayAvailableMovement(float startingMovement) {
		ResetMovement();

		// Create a list of nodes to check for movement and add this starting node (where the character currently is).
		List<Node> nodesToCheck = new List<Node>();
		List<Node> nodesToHighlight = new List<Node>();
		nodesToCheck.Add(graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z]);
		nodesToHighlight.Add(graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z]);

		graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z].isHighlighted = true;

		graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z].movesRemaining = startingMovement;

		// Find all connected nodes with available movement in range.
		while (nodesToCheck.Count > 0) {
			Node n = nodesToCheck[0];

			foreach (Node neighbour in n.neighbours) {
				if (n.movesRemaining - CostToEnterTile(n.x, n.z, neighbour.x, neighbour.z) > 0f) {
					if ((n.movesRemaining - CostToEnterTile(n.x, n.z, neighbour.x, neighbour.z)) > neighbour.movesRemaining) {
						nodesToCheck.Add(neighbour);
						neighbour.movesRemaining = n.movesRemaining - CostToEnterTile(n.x, n.z, neighbour.x, neighbour.z);
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
