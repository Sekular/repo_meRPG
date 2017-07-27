using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType {
	public string m_sName;				// Name of the TileType.
	public GameObject m_tileObject;     // TODO Remove this when ready. Currently for testing purposes only. The object placed in the world to represent this tile.
	public int m_iMoveCost = 1;			// How much it 'costs' to enter this tile. Used for calculating movement over different terrain types.
	public int m_iCoverRating = 0;		// The amount of cover this TileType provides to Actors in nearby tiles.
	public bool m_bIsWalkable = true;	// Can Actors walk on this TileType?
}
