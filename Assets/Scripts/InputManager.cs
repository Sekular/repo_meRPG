using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum InputState
{
	Idle,
	Moving,
	Sprinting,
	Aiming,
	Attacking,
	Reloading
}

public class InputManager : MonoBehaviour
{
	private Grid grid;

    private CombatManager combatManager;

	private InputState currentState = InputState.Idle;
	public TextMesh uiState;

	public GameObject idleIcons;
	public GameObject moveIcons;
	public GameObject sprintIcons;
	public GameObject attackIcons;
	public GameObject reloadIcons;
	
	void Start() {
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
		//UpdateUIState(); // Turning off all UI related code until movement is complete.
	}

	void Update () {
		RunInputs();
		//UIInputs(); // Turning off all UI related code until movement is complete.
	}

	void RunInputs() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (grid.selectedActor) {
				grid.selectedActor.Attack();
				grid.selectedActor.Deselected();
				grid.selectedActor = null;
			}
		}

		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 100.0f)) {
				if (hit.transform.GetComponent<Actor>()) {
					if (hit.transform.GetComponent<Actor>().team == combatManager.activeTeam) {
						if (!hit.transform.GetComponent<Actor>().hasActed) {
							grid.selectedActor = hit.transform.GetComponent<Actor>();
							grid.selectedActor.Selected();
						}
						else {
							Debug.Log("This character has already acted this turn!");
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
							grid.selectedActor.Move(gridSquare.tileX, gridSquare.tileZ);
							grid.selectedActor.hasMoved = true;
						}
						else {
							Debug.Log("This character has already moved.");
						}
					}
				}
			}
		}
	}

	// Turning off all UI related code until movement is complete.
	/*
	void UIInputs()
	{
		uiState.text = currentState.ToString();

		if (currentState == InputState.Idle)
		{
			if(Input.GetKeyDown(KeyCode.Alpha1))
			{
				currentState = InputState.Moving;
				UpdateUIState();
			}
			else if(Input.GetKeyDown(KeyCode.Alpha2))
			{
				currentState = InputState.Aiming;
				UpdateUIState();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				currentState = InputState.Reloading;
				UpdateUIState();
			}
			else if(Input.GetKeyDown(KeyCode.Escape))
			{
				grid.selectedActor.Deselected();
				grid.selectedActor = null;
			}
		}
		else if(currentState == InputState.Moving)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				currentState = InputState.Sprinting;
				UpdateUIState();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
		else if(currentState == InputState.Sprinting)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				currentState = InputState.Moving;
				UpdateUIState();
			}
		}
		else if (currentState == InputState.Aiming)
		{
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				currentState = InputState.Attacking;
				UpdateUIState();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
		else if (currentState == InputState.Attacking)
		{
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				// TODO Fire at current target
				grid.selectedActor.Attack();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
		else if (currentState == InputState.Reloading)
		{
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				// TODO End current actor action
				grid.selectedActor.Reload();
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				currentState = InputState.Idle;
				UpdateUIState();
			}
		}
	}

	public void UpdateUIState()
	{
		switch(currentState)
		{
			case InputState.Idle:
				IdleState();
				break;
			case InputState.Moving:
				MoveState();
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

	void IdleState()
	{
		idleIcons.SetActive(true);
		moveIcons.SetActive(false);
		sprintIcons.SetActive(false);
		attackIcons.SetActive(false);
		reloadIcons.SetActive(false);
	}

	void MoveState()
	{
		idleIcons.SetActive(false);
		moveIcons.SetActive(true);
		sprintIcons.SetActive(false);
		attackIcons.SetActive(false);
		reloadIcons.SetActive(false);
	}

	void SprintingState()
	{
		idleIcons.SetActive(false);
		moveIcons.SetActive(false);
		sprintIcons.SetActive(true);
		attackIcons.SetActive(false);
		reloadIcons.SetActive(false);
	}

	void AimingState()
	{
		idleIcons.SetActive(false);
		moveIcons.SetActive(false);
		sprintIcons.SetActive(false);
		attackIcons.SetActive(true);
		reloadIcons.SetActive(false);
	}

	void AttackingState()
	{
		idleIcons.SetActive(false);
		moveIcons.SetActive(false);
		sprintIcons.SetActive(false);
		attackIcons.SetActive(true);
		reloadIcons.SetActive(false);
	}

	void ReloadingState()
	{
		idleIcons.SetActive(false);
		moveIcons.SetActive(false);
		sprintIcons.SetActive(false);
		attackIcons.SetActive(false);
		reloadIcons.SetActive(true);
	}*/
}
