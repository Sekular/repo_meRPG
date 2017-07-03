using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType {
	public string name;
	public GameObject tileObject;

	public float moveCost = 1;

	public bool isWalkable = true;
}
