using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Team[] m_teams;

    [HideInInspector] public int m_iActiveTeam = 0;

    private int roundNumber = 0;

	  private Grid grid;
	  private InputManager input;

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
        roundNumber++;

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
        float lowestInitiative = 0f;
        int initiativeLoser = 0;
      
        for (int i = 0; i < m_teams.Length; i++) {
            if (Random.Range(0, 100) < lowestInitiative) {
                initiativeLoser = i;
            }
        }

        m_iActiveTeam = initiativeLoser;
        Debug.Log("NEW ROUND: Team " + initiativeLoser + " loses initiative and must move first.");
    }

    public void StartNextTurn() {
        grid.selectedActor = null;

        foreach (Team team in m_teams) {
            foreach (Actor actor in team.m_actors) {
                actor.Deactivate();
            }
        }

        m_iActiveTeam = (m_iActiveTeam++ % m_teams.Length);

        CheckGameOver();
        teamsChecked = 0;
        StartCoroutine(CheckMovesAvailableCR());

        foreach (Actor actor in m_teams[m_iActiveTeam].m_actors) {
            if (!actor.isIncap) {
                if (!actor.hasActed) {
                    actor.SetAvailable();
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
    
    private int teamsChecked = 0;

    IEnumerator CheckMovesAvailableCR() {
        bool isMoveFound = false;
        
        if(teamsChecked == m_teams.Length) {
            Debug.Log("No moves remaining for either team, starting a new round.");
            NewRound();
        }
        
        foreach (Actor actor in m_teams[m_iActiveTeam].m_actors) {
            if (!actor.isIncap) {
                if (!actor.hasActed) {
                    isMoveFound = true;
                }
                actor.Deactivate();
            }
        }

        if(!isMoveFound) {
            m_iActiveTeam = (m_iActiveTeam++ % m_teams.Length);
            Debug.Log("Current team has no moves left, swapping to next team.");
            CheckMovesAvailableCR();
        }
        
        yield return null;
    }

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
