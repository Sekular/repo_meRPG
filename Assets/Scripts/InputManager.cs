using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputState {
	Idle,
	Moving,
	Sprinting,
	Aiming,
	Attacking,
	Reloading
}

public class InputManager : MonoBehaviour {
	private Grid grid;

    private CombatManager combatManager;
	public List<Actor> targets;

	[HideInInspector] public InputState currentState = InputState.Idle;
	
	void Start() {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
		UpdateUIState();
	}

	void Update () {
		RunInputs();
		UIInputs(); 
	}

	void RunInputs() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 100.0f)) {
				if (hit.transform.GetComponent<Actor>()) {
					if (hit.transform.GetComponent<Actor>().team == combatManager.activeTeam) {
						if(grid.selectedActor != null) {
							if (hit.transform.name == grid.selectedActor.name) {
								grid.selectedActor.Deselected();
								grid.selectedActor = null;
							}
						}
						else if (!hit.transform.GetComponent<Actor>().hasActed) {
							grid.selectedActor = hit.transform.GetComponent<Actor>();
							grid.selectedActor.Selected();
						}
						else {
							Debug.Log("This character has already acted this turn!");
						}
					}
					else if (hit.transform.GetComponent<Actor>().isTargeted) {
						grid.selectedActor.Attack(hit.transform.GetComponent<Actor>());
					}
					else {
						Debug.Log("Not on active team!");
					}
				}
				else if (hit.transform.GetComponent<MoveTile>()) {
					if (grid.selectedActor != null) {
						if (!grid.selectedActor.hasMoved) {
							MoveTile gridSquare = hit.transform.GetComponent<MoveTile>();
							grid.ResetMovement();
							grid.selectedActor.Move(gridSquare.tileX, gridSquare.tileZ);
							grid.selectedActor.hasMoved = true;
							currentState = InputState.Idle;
							UpdateUIState();
						}
						else {
							Debug.Log("This character has already moved.");
						}
					}
				}
			}
		}
	}

	void UIInputs()
	{
		Debug.Log(currentState);

		if (currentState == InputState.Idle) {
			if(Input.GetKeyDown(KeyCode.Alpha1)) {
				if (!grid.selectedActor.hasMoved) {
					currentState = InputState.Moving;
					UpdateUIState();
				}
			}
			else if(Input.GetKeyDown(KeyCode.Alpha2)) {
				currentState = InputState.Aiming;
				UpdateUIState();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3)) {
				currentState = InputState.Reloading;
				UpdateUIState();
			}
			else if(Input.GetKeyDown(KeyCode.Escape)) {
				grid.selectedActor.Deselected();
				grid.selectedActor = null;
			}
		}
		else if(currentState == InputState.Moving) {
			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				currentState = InputState.Sprinting;
				UpdateUIState();
			}
			else if (Input.GetKeyDown(KeyCode.Escape)) {
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
		else if(currentState == InputState.Sprinting) {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				currentState = InputState.Moving;
				UpdateUIState();
			}
		}
		else if (currentState == InputState.Aiming) {
			if (Input.GetKeyDown(KeyCode.Alpha2)) {
				currentState = InputState.Attacking;
				UpdateUIState();
			}
			else if (Input.GetKeyDown(KeyCode.Escape)) {
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
		else if (currentState == InputState.Attacking) {
			if (Input.GetKeyDown(KeyCode.Alpha2)) {
				// TODO Fire at current target
			}
			else if (Input.GetKeyDown(KeyCode.Escape)) {
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
		else if (currentState == InputState.Reloading) {
			if (Input.GetKeyDown(KeyCode.Alpha3)) {
				// TODO End current actor action
				grid.selectedActor.Reload();
			}
			else if (Input.GetKeyDown(KeyCode.Escape)) {
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
	}

	void HighlightTargets(float range) {
		targets.Clear();

		foreach (Actor actor in combatManager.team1) {
			if(actor.team != grid.selectedActor.team) {
				if((Vector3.Distance(grid.selectedActor.transform.position, actor.transform.position)) < range) {
					targets.Add(actor);
				}
			}
		}

		foreach (Actor actor in combatManager.team2) {
			if (actor.team != grid.selectedActor.team) {
				if ((Vector3.Distance(grid.selectedActor.transform.position, actor.transform.position)) < range) {
					targets.Add(actor);
				}
			}
		}

		foreach(Actor target in targets) {
			target.SetTargeted();
		}
	}

	void ClearTargets() {
		foreach (Actor target in targets) {
			target.ClearTargeted();
		}
	}

	public void UpdateUIState() {
		switch(currentState) {
			case InputState.Idle:
				IdleState();
				break;
			case InputState.Moving:
				MoveState();
				grid.DisplayAvailableMovement(grid.selectedActor.moveDist);
				break;
			case InputState.Sprinting:
				SprintingState();
				break;
			case InputState.Aiming:
				AimingState();
				break;
			case InputState.Attacking:
				AttackingState();
				break;
			case InputState.Reloading:
				ReloadingState();
				break;
		}
	}

	void IdleState() {
		ClearTargets();
	}

	void MoveState() {
		ClearTargets();
	}

	void SprintingState() {
		ClearTargets();
	}

	void AimingState() {
		HighlightTargets(grid.selectedActor.weapon.range);
	}

	void AttackingState() {
		ClearTargets();
	}

	void ReloadingState() {
		ClearTargets();
	}
}
