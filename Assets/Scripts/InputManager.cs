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

	[HideInInspector] public InputState currentState = InputState.NoSelection;

	public Button moveButton;
	public Button aimButton;
	public Button reloadButton;

	public Canvas characterCanvas;
	public Text cName;
	public Text cShield;
	public Text cHealth;
	public Text cHeat;
	public Image cPortrait;
	
	void Start() {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();

		if(characterCanvas == null || cName == null || cShield == null || cHealth == null || cHeat == null || cPortrait == null) {
			Debug.Log("SelectdCharacterCanvas set-up incorrectly.");
		}

		NoSelection();
	}

	void Update () {
		RunMouseInputs();
		RunKeyboardInputs();
	}

	void HighlightTargets(float range) {
		targets.Clear();

		foreach (Actor actor in combatManager.team1) {
			if (actor.team != grid.selectedActor.team && !actor.isIncap) {
				if ((Vector3.Distance(grid.selectedActor.transform.position, actor.transform.position)) < range) {
					targets.Add(actor);
				}
			}
		}

		foreach (Actor actor in combatManager.team2) {
			if (actor.team != grid.selectedActor.team && !actor.isIncap) {
				if ((Vector3.Distance(grid.selectedActor.transform.position, actor.transform.position)) < range) {
					targets.Add(actor);
				}
			}
		}

		foreach (Actor target in targets) {
			target.SetTargeted();
		}
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
							UpdateCharacterUI();
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
		UpdateCharacterUI();
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
					HighlightTargets(grid.selectedActor.weapon.range);
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
				UpdateCharacterUI();
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

	private void UpdateCharacterUI() {
		if(grid.selectedActor != null) {
			cName.text = grid.selectedActor.name;
			cShield.text = grid.selectedActor.currentShield.ToString();
			cHealth.text = grid.selectedActor.currentHealth.ToString();
			cHeat.text = grid.selectedActor.weapon.currentHeat.ToString();
			cPortrait.sprite = grid.selectedActor.characterPortrait;
			characterCanvas.gameObject.SetActive(true);
		}
		else {
			characterCanvas.gameObject.SetActive(false);
		}
	}
}
