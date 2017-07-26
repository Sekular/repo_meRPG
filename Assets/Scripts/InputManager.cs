using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InputState {
	NoSelection,
	Idle,
	Moving,
	Aiming,
	Reloading
}

public class InputManager : MonoBehaviour {
	private Grid grid;

	private CombatManager combatManager;
	[HideInInspector] public List<Actor> targets;
	[HideInInspector] public List<int> hitChances;

	[HideInInspector] public InputState currentState = InputState.NoSelection;

	public Button moveButton;
	public Button aimButton;
	public Button reloadButton;
	
	void Start() {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();

		NoSelection();
	}

	void Update () {
		RunMouseInputs();
		RunKeyboardInputs();
	}

	void HighlightTargets() {
		targets.Clear();
		hitChances.Clear();
		
		foreach (Actor actor in combatManager.team1) {
			if (actor.team != grid.selectedActor.team && !actor.isIncap) {
				GetTargets(actor);
			}
		}

		foreach (Actor actor in combatManager.team2) {
			if (actor.team != grid.selectedActor.team && !actor.isIncap) {
				GetTargets(actor);
			}
		}

		foreach (Actor target in targets) {
			target.SetTargeted();
		}
	}

	private void GetTargets(Actor actor) {
		RaycastHit hit;
		Vector3 offset = new Vector3(0f, 1.6f, 0f);
		Vector3 checkDir = (actor.transform.position + offset) - (grid.selectedActor.transform.position + offset);
		Debug.DrawLine((actor.transform.position + offset), (grid.selectedActor.transform.position + offset));
		if (Physics.Raycast(grid.selectedActor.transform.position + offset, checkDir, out hit, actor.weapon.m_fRange)) {
			if (hit.collider.name == actor.name) {
				targets.Add(actor);
				hitChances.Add(CalculateHitChance(actor));
			}
		}
	}

	int CalculateHitChance(Actor actor) {
		int r = (int)(60 - ((Vector3.Distance(grid.selectedActor.transform.position, actor.transform.position) / actor.weapon.m_fRange) * 60));
		int c = (40 - (CheckTargetCover(actor) * 20));
		int chanceToHit = r + c;
		Debug.Log(chanceToHit);
		return chanceToHit;
	}

	public int CheckTargetCover(Actor target) {
		Vector3 checkDir = target.transform.position - grid.selectedActor.transform.position;
		int c = 0;

		checkDir.x = (int)checkDir.x;
		checkDir.y = (int)checkDir.y;
		checkDir.z = (int)checkDir.z;

		if (checkDir.z > 0f) {
			if (c <= grid.tileTypes[grid.tiles[target.tileX, target.tileZ - 1]].m_iCoverRating) {
				c = grid.tileTypes[grid.tiles[target.tileX, target.tileZ - 1]].m_iCoverRating;
			}
		}
		else if (checkDir.z < 0f) {
			if (c <= grid.tileTypes[grid.tiles[target.tileX, target.tileZ + 1]].m_iCoverRating) {
				c = grid.tileTypes[grid.tiles[target.tileX, target.tileZ + 1]].m_iCoverRating;
			}
		}
		else {
			if (checkDir.x > 0f) {
				if (c <= grid.tileTypes[grid.tiles[target.tileX - 1, target.tileZ]].m_iCoverRating) {
					c = grid.tileTypes[grid.tiles[target.tileX - 1, target.tileZ]].m_iCoverRating;
				}
			}
			else {
				if (c <= grid.tileTypes[grid.tiles[target.tileX + 1, target.tileZ]].m_iCoverRating) {
					c = grid.tileTypes[grid.tiles[target.tileX + 1, target.tileZ]].m_iCoverRating;
				}
			}
		}

		if (checkDir.x > 0f) {
			if (c <= grid.tileTypes[grid.tiles[target.tileX - 1, target.tileZ]].m_iCoverRating) {
				c = grid.tileTypes[grid.tiles[target.tileX - 1, target.tileZ]].m_iCoverRating;
			}
		}
		else if (checkDir.x < 0f) {
			if (c <= grid.tileTypes[grid.tiles[target.tileX + 1, target.tileZ]].m_iCoverRating) {
				c = grid.tileTypes[grid.tiles[target.tileX + 1, target.tileZ]].m_iCoverRating;
			}
		}
		else {
			if (checkDir.z > 0f) {
				if (c <= grid.tileTypes[grid.tiles[target.tileX, target.tileZ - 1]].m_iCoverRating) {
					c = grid.tileTypes[grid.tiles[target.tileX, target.tileZ - 1]].m_iCoverRating;
				}
			}
			else {
				if (c <= grid.tileTypes[grid.tiles[target.tileX, target.tileZ + 1]].m_iCoverRating) {
					c = grid.tileTypes[grid.tiles[target.tileX, target.tileZ + 1]].m_iCoverRating;
				}
			}
		}

		return c;
	}

	public void ClearTargets() {
		foreach (Actor target in targets) {
			target.ClearTargeted();
		}
	}

	void RunMouseInputs() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 100.0f)) {
				if (hit.transform.GetComponent<Actor>()) {
					if (hit.transform.GetComponent<Actor>().team == combatManager.activeTeam) {
						if(grid.selectedActor != null) {
							if (hit.transform.name == grid.selectedActor.name && currentState == InputState.Idle) {
								if(grid.selectedActor.hasMoved || grid.selectedActor.hasActed) {
									return;
								}
								else {
									NoSelection();
								}
							}
						}
						else if (!hit.transform.GetComponent<Actor>().hasActed) {
							grid.selectedActor = hit.transform.GetComponent<Actor>();
							currentState = InputState.Idle;
							UpdateInputUI();
							grid.selectedActor.Selected();
						}
						else {
							Debug.Log("This character has already acted this turn!");
						}
					}
					else if (hit.transform.GetComponent<Actor>().isTargeted) {
						grid.selectedActor.Attack(hit.transform.GetComponent<Actor>(), grid.selectedActor);
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
							Idle();
						}
						else {
							Debug.Log("This character has already moved.");
						}
					}
				}
			}
		}
	}

	void RunKeyboardInputs() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			Move();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			Aim();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3)) {
			Reload();
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			switch (currentState) {
				case InputState.NoSelection:
					break;
				case InputState.Idle:
					if (grid.selectedActor && !grid.selectedActor.hasActed && !grid.selectedActor.hasMoved) {
						grid.selectedActor.Deselected();
						grid.selectedActor = null;
						currentState = InputState.NoSelection;
						UpdateInputUI();
					}
					break;
				case InputState.Moving:
					if (grid.selectedActor) {
						Idle();
					}
					break;
				case InputState.Aiming:
					if (grid.selectedActor) {
						Idle();
					}
					break;
				case InputState.Reloading:
					if (grid.selectedActor) {
						Idle();
					}
					break;
			}
		}
	}

	private void NoSelection() {
		if (grid.selectedActor) {
			grid.selectedActor.Deselected();
			ClearTargets();
			grid.selectedActor = null;
		}

		currentState = InputState.NoSelection;
		UpdateInputUI();
	}

	private void Idle() {
		if (grid.selectedActor) {
			currentState = InputState.Idle;
			ClearTargets();
			grid.ResetMovement();
			UpdateInputUI();
		}
	}

	private void Move() {
		ClearTargets();
		if (grid.selectedActor) {
			grid.selectedActor.ReloadDeactive();
			if (!grid.selectedActor.hasMoved && !grid.selectedActor.hasActed) {
				if (currentState != InputState.Moving) {
					grid.DisplayAvailableMovement(grid.selectedActor.moveDist);
					currentState = InputState.Moving;
					UpdateInputUI();
				}
				else {
					grid.ResetMovement();
					currentState = InputState.Idle;
					UpdateInputUI();
				}
			}
		}
	}

	private void Aim() {
		grid.ResetMovement();
		if (grid.selectedActor) {
			grid.selectedActor.ReloadDeactive();
			if (!grid.selectedActor.hasActed) {
				if (currentState != InputState.Aiming) {
					HighlightTargets();
					currentState = InputState.Aiming;
					UpdateInputUI();
				}
				else {
					ClearTargets();
					currentState = InputState.Idle;
					UpdateInputUI();
				}
			}
		}
	}

	private void Reload() {
		grid.ResetMovement();
		ClearTargets();

		if (grid.selectedActor) {
			if (!grid.selectedActor.hasActed) {
				if (currentState != InputState.Reloading) {
					grid.selectedActor.ReloadActive();
					currentState = InputState.Reloading;
					UpdateInputUI();
				}
				else {
					grid.selectedActor.ReloadDeactive();
					currentState = InputState.Idle;
					UpdateInputUI();
				}
			}
		}
	}

	private void UpdateInputUI() {
		switch(currentState) {
			case InputState.NoSelection:
				moveButton.interactable = false;
				aimButton.interactable = false;
				reloadButton.interactable = false;
				break;
			case InputState.Idle:
				if (!grid.selectedActor.hasMoved) {
					moveButton.interactable = true;
				}
				if (!grid.selectedActor.hasActed) {
					aimButton.interactable = true;
					reloadButton.interactable = true;
				}
				break;
			case InputState.Moving:
				moveButton.interactable = false;
				if (!grid.selectedActor.hasActed) {
					aimButton.interactable = true;
					reloadButton.interactable = true;
				}
				break;
			case InputState.Aiming:
				aimButton.interactable = false;
				if (!grid.selectedActor.hasMoved) {
					moveButton.interactable = true;
				}
				if (!grid.selectedActor.hasActed) {
					reloadButton.interactable = true;
				}
				break;
			case InputState.Reloading:
				reloadButton.interactable = false;
				if (!grid.selectedActor.hasMoved) {
					moveButton.interactable = true;
				}
				if (!grid.selectedActor.hasActed) {
					aimButton.interactable = true;
				}
				break;
		}
	}
}
