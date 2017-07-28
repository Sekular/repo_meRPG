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
        foreach (Actor actor in team1) {
            actor.ResetActions();
        }
      
        foreach (Actor actor in team2) {
            actor.ResetActions();
        }
    }

    void RollInitiative() {
        if (Random.Range(0, 100) > 50f) {
            activeTeam = 2;
            //Debug.Log("NEW ROUND: Team 1 wins initiative, Team 2 moves first.");
        }
        else {
            activeTeam = 1;
            //Debug.Log("NEW ROUND: Team 2 wins initiative, Team 1 moves first.");
        }
    }

    public void StartNextTurn() {
		grid.selectedActor = null;

		foreach (Actor actor in team2) { actor.Deactivate(); }
		foreach (Actor actor in team1) { actor.Deactivate(); }

		if (activeTeam == 2) {
			activeTeam = 1;
		} else if (activeTeam == 1) {
			activeTeam = 2;
		}

		CheckGameOver();
        CheckMovesAvailable();

        if(activeTeam == 1) {
			foreach (Actor actor in team1) {
				if (!actor.isIncap) {
					if (!actor.hasActed) {
						actor.SetAvailable();
					}
				}
			}
		}
        else if(activeTeam == 2) {
            foreach (Actor actor in team2) {
                if (!actor.isIncap) {
                    if (!actor.hasActed) {
						actor.SetAvailable();
					}
                }
            }
        }
    }

	public void DeactivateTeammates(int team, Actor currentActor) {
		if(team == 1) {
			foreach (Actor actor in team1) {
				if(actor != currentActor) {
					actor.Deactivate();
				}
			}
		}
		else if(team == 2) {
			foreach (Actor actor in team2) {
				if (actor != currentActor) {
					actor.Deactivate();
				}
			}
		}
		else {
			Debug.Log("No valid team received!");
		}
	}

    void CheckMovesAvailable() {
        if (activeTeam == 1) {
            int t = 0;

            foreach (Actor actor in team1) {
                if (!actor.isIncap) {
                    if (!actor.hasActed) {
                        t += 1;
                    }
                }
            }

            if (t == 0) {
                activeTeam = 2;
                Debug.Log("Team 1 has no moves left, swapping to Team 2.");

                t = 0;

                foreach (Actor actor in team2) {
                    if (!actor.isIncap) {
                        if (!actor.hasActed) {
                            t += 1;
                        }
                    }
                }

                if (t == 0) {
                    Debug.Log("No moves remaining for either team, starting a new round.");
                    NewRound();
                }
            }
        }
        else if (activeTeam == 2) {
            int t = 0;

            foreach (Actor actor in team2) {
                if (!actor.isIncap) {
                    if (!actor.hasActed) {
                        t += 1;
                    }
                }
            }

            if (t == 0) {
                activeTeam = 1;
                Debug.Log("Team 2 has no moves left, swapping to Team 1.");

                t = 0;

                foreach (Actor actor in team1) {
                    if (!actor.isIncap) {
                        if (!actor.hasActed) {
                            t += 1;
                        }
                    }
                }

                if (t == 0) {
                    Debug.Log("No moves remaining for either team, starting a new round.");
                    NewRound();
                }
            }
        }
    }

    bool CheckGameOver() {
        int t = 0;

        foreach (Actor actor in team1) {
            if (actor.isIncap) {
                t += 1;
            }
        }

        if(t == team1.Length) {
            Debug.Log("GAME OVER: Team 1 incapacitated.");
            return true;
        }
        else {
            int s = 0;

            foreach (Actor actor in team2) {
                if (actor.isIncap) {
                    s += 1;
                }
            }

            if (s == team2.Length) {
                Debug.Log("GAME OVER: Team 2 incapacitated.");
                return true;
            }
            else {
                return false;
            }
        }
    }
}
