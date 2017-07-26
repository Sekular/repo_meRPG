using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType {
	public string m_sName;
	public GameObject m_tileObject;
	public int m_iMoveCost = 1;
	public int m_iCoverRating = 0;
	public bool m_bIsWalkable = true;
}
