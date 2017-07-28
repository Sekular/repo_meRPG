using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour {
	

	public Texture2D m_levelTemplate;				// Bitmap used to create the functional layer of the level.
	public int m_iMapSizeX, m_iMapSizeZ;			// The size of the map to create.
	public TileType[] tileTypes;                    // TODO TileType change from an Array to an Enum.
													// Array of TileTypes that information on how the tile effects movement and what
													// visual to display for the respective tile. Visual.

	public NodeVisual m_moveVisual;					// Object used to display potential movement for an Actor.
	[HideInInspector] public Actor selectedActor;	// The currently selected Actor.
	[HideInInspector] public int[,] tiles;          // Array of tile data. Non-visual.
	[HideInInspector] public Node[,] graph;			// Array of Nodes that contain references to their neighbours. Non-visual.

	// Converts tile values into world position.
	public Vector3 TileToWorld(int x, int z) { return new Vector3(x, 0, z); }
	// Checks if an Actor can enter a tile based on the TileType v MovementType
	public bool ValidTile(int x, int z) { return tileTypes[tiles[x, z]].m_bIsWalkable; }

	void Start() {
		GenerateMapData();
		GenerateGraph();
		GenerateGrid();
	}
	
	// Initializes the basic map data.
	void GenerateMapData() {
		// Allocate map tiles
		tiles = new int[m_iMapSizeX, m_iMapSizeZ];

		// Get pixel information from the level template.
		Color32[] pixels = m_levelTemplate.GetPixels32();
		int p = 0;

		// Initialize map tiles from pixel information.
		for (int z = 0; z < m_iMapSizeZ; z++) {
			for (int x = 0; x < m_iMapSizeZ; x++) {
				if (pixels[p] == Color.black) {
					tiles[x, z] = 2;
				}
				else if (pixels[p] == Color.blue) {
					tiles[x, z] = 1;
				}
				else {
					tiles[x, z] = 0;
				}
				p++;
			}
		}
	}

	// Returns the cost to enter the tile coordinates parsed.
	public float CostToEnterTile(int sourceX, int sourceZ, int targetX, int targetZ) {
		TileType tt = tileTypes[ tiles[targetX, targetZ] ];

		if(!ValidTile(targetX, targetZ)) {
			return Mathf.Infinity;
		}

		float cost = tt.m_iMoveCost;

		// Detect diagonal movement
		if (sourceX != targetX && sourceZ != targetZ) {	cost += 0.01f; }

		return cost;
	}

	// Generates the grid graph for path-finding. Creates all Nodes in the grid.
	void GenerateGraph() {
		// Initialize the array.
		graph = new Node[m_iMapSizeX, m_iMapSizeZ];

		// Initialize a Node for each array location.
		for (int x = 0; x < m_iMapSizeX; x++) {
			for (int z = 0; z < m_iMapSizeZ; z++) {
				graph[x, z] = new Node();
				graph[x, z].x = x;
				graph[x, z].z = z;

				graph[x, z].m_moveVisual = (NodeVisual)Instantiate(m_moveVisual, new Vector3(x, 0.05f, z), Quaternion.identity);
				graph[x, z].m_moveVisual.Deactivate();
			}
		}

		// Calculate all neighbours (Grid must be constructed first).
		for (int x = 0; x < m_iMapSizeX; x++) {
			for (int z = 0; z < m_iMapSizeZ; z++) {
				// 3 left neighbours.
				if (x > 0) {
					graph[x, z].neighbours.Add(graph[x - 1, z]);
					graph[x, z].covWest = tileTypes[tiles[x - 1, z]].m_iCoverRating;
					if (z > 0) { graph[x, z].neighbours.Add(graph[x - 1, z - 1]); }
					if (z < m_iMapSizeZ - 1) { graph[x, z].neighbours.Add(graph[x - 1, z + 1]); }
				}

				// 3 right neighbours.
				if (x < m_iMapSizeX - 1) {
					graph[x, z].neighbours.Add(graph[x + 1, z]);
					graph[x, z].covEast = tileTypes[tiles[x + 1, z]].m_iCoverRating;
					if (z > 0) { graph[x, z].neighbours.Add(graph[x + 1, z - 1]); }
					if (z < m_iMapSizeZ - 1) { graph[x, z].neighbours.Add(graph[x + 1, z + 1]); }
				}

				// Up and down.
				if (z > 0) {
					graph[x, z].neighbours.Add(graph[x, z - 1]);
					graph[x, z].covSouth = tileTypes[tiles[x, z - 1]].m_iCoverRating;
				}
				if (z < m_iMapSizeZ - 1) {
					graph[x, z].neighbours.Add(graph[x, z + 1]);
					graph[x, z].covNorth = tileTypes[tiles[x, z + 1]].m_iCoverRating;
				}
			}
		}
	}

  // TODO Remove when no longer required. Initializes MoveTiles and generates a physical representation of the movement grid and blockers.
	void GenerateGrid() {
		for (int x = 0; x < m_iMapSizeX; x++) {
			for (int z = 0; z < m_iMapSizeZ; z++) {
				TileType t = tileTypes[tiles[x, z]];

				GameObject go = Instantiate(t.m_tileObject, new Vector3(x, 0f, z), Quaternion.identity);

				MoveTile mt = go.GetComponent<MoveTile>();
        mt.m_iTileX = x;
        mt.m_iTileZ = z;
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
		//nodesToHighlight.Add(graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z]);

		graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z].m_bIsHighlighted = true;

		graph[(int)selectedActor.transform.position.x, (int)selectedActor.transform.position.z].m_fMovesRemaining = startingMovement;

		// Find all connected nodes with available movement in range.
		while (nodesToCheck.Count > 0) {
			Node n = nodesToCheck[0];

			foreach (Node neighbour in n.neighbours) {
				if (n.m_fMovesRemaining - CostToEnterTile(n.x, n.z, neighbour.x, neighbour.z) > 0f) {
					if ((n.m_fMovesRemaining - CostToEnterTile(n.x, n.z, neighbour.x, neighbour.z)) > neighbour.m_fMovesRemaining) {
						nodesToCheck.Add(neighbour);
						neighbour.m_fMovesRemaining = n.m_fMovesRemaining - CostToEnterTile(n.x, n.z, neighbour.x, neighbour.z);
						if (!neighbour.m_bIsHighlighted) {
							nodesToHighlight.Add(neighbour);
						}
					}
				}
			}

			nodesToCheck.Remove(n);
		}
		
		foreach (Node node in nodesToHighlight) {
			node.m_moveVisual.Activate();
		}
	}

	public void ResetMovement() {
		foreach(Node node in graph) {
			node.m_fMovesRemaining = 0;
			node.m_bIsHighlighted = false;
			node.m_moveVisual.Deactivate();
		}
	}
}
