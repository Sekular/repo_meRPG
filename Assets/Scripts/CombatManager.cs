using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
	public Team[] m_teams;		// An array of all teams on this level.
	[HideInInspector] public int m_iActiveTeam = 0;		// A reference to the currently active team.

	private Grid grid;		// A reference to the grid object on this level.
	private InputManager input;		// A reference to the InputManager on this level.

	void Awake() {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		input = GameObject.Find("InputManager").GetComponent<InputManager>();
		SetTeams();
	}

	void Start() {
		NewRound();
	}

	void SetTeams() {
		int i = 0;
      
		foreach (Team team in m_teams) {
			team.Init(i);
				i++;
		}
	}
	
	void NewRound() {
		input.ClearTargets();
		ResetActors();
		RollInitiative();
		StartNextTurn();
	}

	void ResetActors() {
		foreach (Team team in m_teams) {
			foreach (Actor actor in team.m_actors) {
				actor.ResetActions();
			}
		}
	}
	
	void RollInitiative() {
		float lowestInitiative = 101f;
		int initiativeLoser = 0;

		for (int i = 0; i < m_teams.Length; i++) {
			float initiative = Random.Range(0, 100);

			if (initiative < lowestInitiative) {
				initiativeLoser = i;
				lowestInitiative = initiative;
			}
		}

		m_iActiveTeam = initiativeLoser;
		//Debug.Log("NEW ROUND: Team " + initiativeLoser + " loses initiative and must move first.");
	}

	public void StartNextTurn() {
		if (!CheckGameOver()) {
			grid.selectedActor = null;

			foreach (Team team in m_teams) {
				foreach (Actor actor in team.m_actors) {
					actor.Deactivate();
				}
			}

			m_iActiveTeam++;
			m_iActiveTeam = (m_iActiveTeam % m_teams.Length);

			teamsChecked = 0;
			CheckMovesAvailable();

			foreach (Actor actor in m_teams[m_iActiveTeam].m_actors) {
				if (!actor.isIncap) {
					if (!actor.hasActed) {
						actor.SetAvailable();
					}
				}
			}
		}
	}

	public void DeactivateTeammates(Actor currentActor) {
		foreach (Actor actor in m_teams[m_iActiveTeam].m_actors) {
			if (actor != currentActor) {
				actor.Deactivate();
			}
		}
	}

	private int teamsChecked = 0;	// Used as an iterator for the CheckMovesAvailable function.

	void CheckMovesAvailable() {
		bool isMoveFound = false;

		// If every team has used all of their moves, start a new round.
		if (teamsChecked == m_teams.Length) { NewRound(); }

		foreach (Actor actor in m_teams[m_iActiveTeam].m_actors) {
			if (!actor.isIncap) {
				if (!actor.hasActed) {
					isMoveFound = true;
				}
				actor.Deactivate();
			}
		}

		// If no moves are found, move to the next team and check for available moves.
		if (!isMoveFound) {
			m_iActiveTeam++;
			m_iActiveTeam = (m_iActiveTeam % m_teams.Length);
			teamsChecked++;
			CheckMovesAvailable();
		}
    }

	// Checks to see if all but a single team has been fully incapacitated.
	bool CheckGameOver() {
		int teamsDown = 0;

		foreach (Team team in m_teams) {
			int incaps = 0;

			foreach (Actor actor in team.m_actors) {
				if (actor.isIncap) {
					incaps++;
				}
			}

			if (incaps == team.m_actors.Count) {
				teamsDown++;
			}
		}

		if (teamsDown >= (m_teams.Length - 1)) {
			Debug.Log("GAME OVER!");
			return true;
		}
		else {
			return false;
		}
	}
}
