﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour {
	[HideInInspector] public List<Actor> m_actors = new List<Actor>();		// List of actors on the team.

	public void Init (int i_iTeamNumber) {
		Actor[] tActors = GetComponentsInChildren<Actor>();

		foreach (Actor a in tActors) {
			a.actorTeam = i_iTeamNumber;
			m_actors.Add(a);
		}
	}
}
