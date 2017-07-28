using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour {

  public List<Actor> m_actors = new List<Actor>();
  [HideInInspector] public int m_iTeam = 0;
  
  public void Init (int i_iTeamNumber) {
    Actor[] tActors = GetComponentsInChildren<Actor>();

    foreach (Actor a in tActors) {
      a.team = i_iTeamNumber;
      m_actors.Add(a);
    }
	}
}
