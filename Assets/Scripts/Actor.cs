using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor : MonoBehaviour {
	//___ANIMATION______________________________________________//
	private Animator anim;

	//___MOVEMENT_______________________________________________//
	[HideInInspector] public int tileX, tileY;
	[HideInInspector] public Grid grid;
	[HideInInspector] public List<Node> currentPath = null;

	public float moveSpeed = 5;


	//___COMBAT_________________________________________________//
	private CombatManager combatManager;
	private Weapon weapon;

	private bool isAttacking;
	public float aimDelay;
	public float fireTime;

	public int maxHealth;
	private int currentHealth;

	//___MANAGEMENT_ ___________________________________________//
	[HideInInspector] public bool isAwaitingOrders = false;
	[HideInInspector] public bool hasActed = false;
	[HideInInspector] public bool hasMoved = false;
	[HideInInspector] public bool isIncap = false;
	[HideInInspector] public int team = 0;

	public GameObject availableIcon;
	public GameObject selectedIcon;

	private InputManager input;

	void Start() {
		anim = GetComponentInChildren<Animator>();
		weapon = GetComponentInChildren<Weapon>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		input = GameObject.Find("Main Camera").GetComponent<InputManager>();

		availableIcon.SetActive(false);
		selectedIcon.SetActive(false);

		currentHealth = maxHealth;
	}

	public void ResetActions() {
		hasActed = false;
		isIncap = false;
		hasMoved = false;
	}


	// This is currently drawing a line on click in conjunction with MoveTile.OnMouseUp(). Use this again later for showing the movement of characters.
	/*void Update() {
		if (currentPath != null) {
			int currNode = 0;

			while (currNode < currentPath.Count - 1) {
				Vector3 start = grid.TileToWorld(currentPath[currNode].x, currentPath[currNode].y);
				Vector3 end = grid.TileToWorld(currentPath[currNode + 1].x, currentPath[currNode + 1].y);

				Debug.DrawLine(start, end, Color.red);

				currNode++;
			}
		}
	}*/

	void FixedUpdate()
    {
        if (isAwaitingOrders && !hasActed && !isIncap)
        {
            if (combatManager.activeTeam == team)
            {
                availableIcon.SetActive(true);
            }
            else
            {
                availableIcon.SetActive(false);
            }
        }
        else
        {
            availableIcon.SetActive(false);
        }
    }

    public void Selected()
    {
        selectedIcon.SetActive(true);
    }

    public void Deselected()
    {
        selectedIcon.SetActive(false);
    }

	public void Move() {
		float remainingMovement = moveSpeed;

		anim.SetBool("IsMoving", true);

		while (remainingMovement > 0) {
			if (currentPath == null) { return; }

			// Get/take cost to move to next tile
			remainingMovement -= grid.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

			// Move Actor
			tileX = currentPath[1].x;
			tileY = currentPath[1].y;
			transform.position = grid.TileToWorld(tileX, tileY);

			// Remove current node from path (you've already reached it)
			currentPath.RemoveAt(0);

			// Reached destination, clear path-finding information
			if (currentPath.Count == 1) { currentPath = null; }
		}
	}

    public void Attack()
    {
        StartCoroutine(AttackCR());
    }

    public IEnumerator AttackCR()
    {
        anim.SetBool("IsAiming", true);
        yield return new WaitForSeconds(aimDelay);
        weapon.Fire();
        yield return new WaitForSeconds(fireTime);
        anim.SetBool("IsAiming", false);
        isAttacking = false;

        hasActed = true;

        CycleActiveTeam();

        combatManager.StartNextTurn();
    }

    void CycleActiveTeam()
    {
        if (combatManager.activeTeam == 2)
        {
            combatManager.activeTeam = 1;
        }
        else if (combatManager.activeTeam == 1)
        {
            combatManager.activeTeam = 2;
        }
    }

	public void Reload()
	{
		weapon.currentHeat = 0;
	}

	public void TakeDamage(int damage)
	{
		currentHealth -= damage;

		if(currentHealth <= 0)
		{
			isIncap = true;
			Debug.Log(gameObject.name + " has been incapacitated.");

			// TODO Play death animation.
		}
	}
}
