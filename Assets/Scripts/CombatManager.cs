using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Actor[] team1;
    public Actor[] team2;

    [HideInInspector] public int activeTeam = 0;

    private int roundNumber = 0;

	void Start()
    {
        SetTeams();
        NewRound();
	}
	
    void SetTeams()
    {
        foreach (Actor actor in team1)
        {
            actor.team = 1;
        }

        foreach (Actor actor in team2)
        {
            actor.team = 2;
        }
    }

    void NewRound()
    {
        roundNumber++;
        Debug.Log("Starting Round " + roundNumber.ToString());

        ResetActors();
        RollInitiative();
        StartNextTurn();
    }

    void ResetActors()
    {
        foreach (Actor actor in team1)
        {
            actor.ResetActions();
        }
      
        foreach (Actor actor in team2)
        {
            actor.ResetActions();
        }
    }

    void RollInitiative()
    {
        if (Random.Range(0, 100) > 50f)
        {
            activeTeam = 2;
            Debug.Log("NEW ROUND: Team 1 wins initiative, Team 2 moves first.");
        }
        else
        {
            activeTeam = 1;
            Debug.Log("NEW ROUND: Team 2 wins initiative, Team 1 moves first.");
        }
    }

    public void StartNextTurn()
    {
        CheckGameOver();
        CheckMovesAvailable();

        if(activeTeam == 1)
        {
            Debug.Log("NEW TURN: Team 1.");

            foreach (Actor actor in team1)
            {
                if (!actor.isIncap)
                {
                    if (!actor.hasActed)
                    {
                        actor.isAwaitingOrders = true;
                        Debug.Log(actor.name + " awaiting orders.");
                    }
                }
            }
        }
        else if(activeTeam == 2)
        {
            Debug.Log("NEW TURN: Team 2.");

            foreach (Actor actor in team2)
            {
                if (!actor.isIncap)
                {
                    if (!actor.hasActed)
                    {
                        actor.isAwaitingOrders = true;
                        Debug.Log(actor.name + " awaiting orders.");
                    }
                }
            }
        }
    }

    void CheckMovesAvailable()
    {
        if (activeTeam == 1)
        {
            int t = 0;

            foreach (Actor actor in team1)
            {
                if (!actor.isIncap)
                {
                    if (!actor.hasActed)
                    {
                        t += 1;
                    }
                }
            }

            if (t == 0)
            {
                activeTeam = 2;
                Debug.Log("Team 1 has no moves left, swapping to Team 2.");

                t = 0;

                foreach (Actor actor in team2)
                {
                    if (!actor.isIncap)
                    {
                        if (!actor.hasActed)
                        {
                            t += 1;
                        }
                    }
                }

                if (t == 0)
                {
                    Debug.Log("No moves remaining for either team, starting a new round.");
                    NewRound();
                }
            }
        }
        else if (activeTeam == 2)
        {
            int t = 0;

            foreach (Actor actor in team2)
            {
                if (!actor.isIncap)
                {
                    if (!actor.hasActed)
                    {
                        t += 1;
                    }
                }
            }

            if (t == 0)
            {
                activeTeam = 1;
                Debug.Log("Team 2 has no moves left, swapping to Team 1.");

                t = 0;

                foreach (Actor actor in team1)
                {
                    if (!actor.isIncap)
                    {
                        if (!actor.hasActed)
                        {
                            t += 1;
                        }
                    }
                }

                if (t == 0)
                {
                    Debug.Log("No moves remaining for either team, starting a new round.");
                    NewRound();
                }
            }
        }


    }

    bool CheckGameOver()
    {
        int t = 0;

        foreach (Actor actor in team1)
        {
            if (actor.isIncap)
            {
                t += 1;
            }
        }

        if(t == team1.Length)
        {
            Debug.Log("GAME OVER: Team 1 incapacitated.");
            return true;
        }
        else
        {
            int s = 0;

            foreach (Actor actor in team2)
            {
                if (actor.isIncap)
                {
                    s += 1;
                }
            }

            if (s == team2.Length)
            {
                Debug.Log("GAME OVER: Team 2 incapacitated.");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
