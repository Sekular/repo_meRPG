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
	public Button m_moveButton;		// The UI button for moving.
	public Button m_aimButton;		// The UI button for aiming.
	public Button m_reloadButton;		// The UI button for reloading.
	[HideInInspector] public InputState m_currentState = InputState.NoSelection;	// The current InputState.

	private Grid grid;      // A reference to the Grid on this level.
	private CombatManager combatManager;	// A reference to the CombatManager on this level.
	
	void Start() {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
		NoSelection();
	}

	void Update () {
		RunMouseInputs();
		RunKeyboardInputs();
	}

	void RunMouseInputs() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 100.0f)) {
				if (hit.transform.GetComponent<Actor>()) {
					if (hit.transform.GetComponent<Actor>().actorTeam == combatManager.m_iActiveTeam) {
						if(grid.selectedActor != null) {
							if (hit.transform.name == grid.selectedActor.name && m_currentState == InputState.Idle) {
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
							m_currentState = InputState.Idle;
							UpdateInputUI();
							grid.selectedActor.Selected();
						}
						else {
							Debug.Log("This character has already acted this turn!");
						}
					}
					else if (hit.transform.GetComponent<Actor>().isTargeted) {
						if (grid.selectedActor != null) {
							grid.selectedActor.Attack(hit.transform.GetComponent<Actor>(), grid.selectedActor);
						}
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
							grid.selectedActor.Move(gridSquare.m_iTileX, gridSquare.m_iTileZ);
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

		if (Input.GetKeyDown(KeyCode.Return)) {
			if(m_currentState == InputState.Reloading) {
				grid.selectedActor.Reload();
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			switch (m_currentState) {
				case InputState.NoSelection:
					break;
				case InputState.Idle:
					if (grid.selectedActor && !grid.selectedActor.hasActed && !grid.selectedActor.hasMoved) {
						grid.selectedActor.Deselected();
						grid.selectedActor = null;
						m_currentState = InputState.NoSelection;
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
			combatManager.ClearTargets();
			grid.selectedActor = null;
		}

		m_currentState = InputState.NoSelection;
		UpdateInputUI();
	}

	private void Idle() {
		if (grid.selectedActor) {
			m_currentState = InputState.Idle;
			combatManager.ClearTargets();
			grid.ResetMovement();
			UpdateInputUI();
		}
	}

	private void Move() {
		combatManager.ClearTargets();
		if (grid.selectedActor) {
			grid.selectedActor.ReloadDeactive();
			if (!grid.selectedActor.hasMoved && !grid.selectedActor.hasActed) {
				if (m_currentState != InputState.Moving) {
					grid.DisplayAvailableMovement(grid.selectedActor.moveDist);
					m_currentState = InputState.Moving;
					UpdateInputUI();
				}
				else {
					grid.ResetMovement();
					m_currentState = InputState.Idle;
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
				if (m_currentState != InputState.Aiming) {
					combatManager.HighlightTargets();
					m_currentState = InputState.Aiming;
					UpdateInputUI();
				}
				else {
					combatManager.ClearTargets();
					m_currentState = InputState.Idle;
					UpdateInputUI();
				}
			}
		}
	}

	private void Reload() {
		grid.ResetMovement();
		combatManager.ClearTargets();

		if (grid.selectedActor) {
			if (!grid.selectedActor.hasActed) {
				if (m_currentState != InputState.Reloading) {
					grid.selectedActor.ReloadActive();
					m_currentState = InputState.Reloading;
					UpdateInputUI();
				}
				else {
					grid.selectedActor.ReloadDeactive();
					m_currentState = InputState.Idle;
					UpdateInputUI();
				}
			}
		}
	}

	private void UpdateInputUI() {
		switch(m_currentState) {
			case InputState.NoSelection:
				m_moveButton.interactable = false;
				m_aimButton.interactable = false;
				m_reloadButton.interactable = false;
				break;
			case InputState.Idle:
				if (!grid.selectedActor.hasMoved) {
					m_moveButton.interactable = true;
				}
				if (!grid.selectedActor.hasActed) {
					m_aimButton.interactable = true;
					m_reloadButton.interactable = true;
				}
				break;
			case InputState.Moving:
				m_moveButton.interactable = false;
				if (!grid.selectedActor.hasActed) {
					m_aimButton.interactable = true;
					m_reloadButton.interactable = true;
				}
				break;
			case InputState.Aiming:
				m_aimButton.interactable = false;
				if (!grid.selectedActor.hasMoved) {
					m_moveButton.interactable = true;
				}
				if (!grid.selectedActor.hasActed) {
					m_reloadButton.interactable = true;
				}
				break;
			case InputState.Reloading:
				m_reloadButton.interactable = false;
				if (!grid.selectedActor.hasMoved) {
					m_moveButton.interactable = true;
				}
				if (!grid.selectedActor.hasActed) {
					m_moveButton.interactable = true;
				}
				break;
		}
	}
}
