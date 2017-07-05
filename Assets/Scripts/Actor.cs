using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor : MonoBehaviour {
	//___ANIMATION______________________________________________//
	private Animator anim;

	//___MOVEMENT_______________________________________________//
	[HideInInspector] public int tileX, tileZ;
	[HideInInspector] public Grid grid;
	[HideInInspector] public List<Node> currentPath = null;
	private Vector3 lerpStart;
	float moveStep = 0;

	[HideInInspector] public bool isMoving;

	public float moveDist = 5;
	public float moveSpeed = 1;
	public float aimSpeed = 1;

	//___COMBAT_________________________________________________//
	private CombatManager combatManager;
	[HideInInspector] public Weapon weapon;

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
	public GameObject targetedIcon;

	[HideInInspector] public bool isTargeted;

	private InputManager input;

	void Start() {
		anim = GetComponentInChildren<Animator>();
		weapon = GetComponentInChildren<Weapon>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		input = GameObject.Find("Main Camera").GetComponent<InputManager>();

		availableIcon.SetActive(false);
		selectedIcon.SetActive(false);
		targetedIcon.SetActive(false);

		currentHealth = maxHealth;
	}

	public void ResetActions() {
		hasActed = false;
		isIncap = false;
		hasMoved = false;
	}

	// This is currently drawing a line on click in conjunction with MoveTile.OnMouseUp(). Use this again later for showing the movement of characters.
	public void ShowPathTo() {
		if (currentPath != null) {
			int currNode = 0;

			while (currNode < currentPath.Count - 1) {
				Vector3 start = grid.TileToWorld(currentPath[currNode].x, currentPath[currNode].z);
				Vector3 end = grid.TileToWorld(currentPath[currNode + 1].x, currentPath[currNode + 1].z);

				Debug.DrawLine(start, end, Color.red);

				currNode++;
			}
		}
	}

	void FixedUpdate() {
        if (isAwaitingOrders && !hasActed && !isIncap) {
            if (combatManager.activeTeam == team) {
                availableIcon.SetActive(true);
            }
            else {
                availableIcon.SetActive(false);
            }
        }
		else {
            availableIcon.SetActive(false);
        }
		
    }

    public void Selected() {
        selectedIcon.SetActive(true);
	}

    public void Deselected() {
        selectedIcon.SetActive(false);
    }

	public void Move(int x, int z) {
		isMoving = true;
		anim.SetBool("IsMoving", true);
		grid.GeneratePathTo(x, z);

		tileX = currentPath[1].x;
		tileZ = currentPath[1].z;
		lerpStart = transform.position;

		StartCoroutine(MoveCR());
	}

	IEnumerator MoveCR() {
		AimAt(grid.TileToWorld(tileX, tileZ));

		while (isMoving) {
			if (Vector3.Distance(transform.position, grid.TileToWorld(tileX, tileZ)) < 0.01f) {
				AdvancePathing();
			}
			
			moveStep += Time.deltaTime * moveSpeed;

			// Move Actor
			transform.position = Vector3.Lerp(lerpStart, grid.TileToWorld(tileX, tileZ), moveStep);
			yield return null;
		}

		anim.SetBool("IsMoving", false);
	}

	// Advances path-finding progress by one node.
	void AdvancePathing() {
		if (currentPath == null) { return; }

		if (currentPath.Count == 1) {
			// We only have one tile left in the path, and that tile MUST be our ultimate
			// destination -- and we are standing on it!
			// So let's just clear our path-finding info.
			isMoving = false;
			currentPath = null;
			return;
		}

		// Move us to our correct "current" position, in case we
		// haven't finished the animation yet.
		transform.position = grid.TileToWorld(tileX, tileZ);

		// Move us to the next tile in the sequence
		tileX = currentPath[1].x;
		tileZ = currentPath[1].z;
		moveStep = 0f;
		lerpStart = transform.position;
		AimAt(grid.TileToWorld(tileX, tileZ));

		// Remove the old "current" tile from the path-finding list
		currentPath.RemoveAt(0);
	}

	public void Attack(Actor target)
    {
        StartCoroutine(AttackCR(target));
    }

    public IEnumerator AttackCR(Actor target)
    {
		AimAt(target.transform.position);

		anim.SetBool("IsAiming", true);
        yield return new WaitForSeconds(aimDelay);
        weapon.Fire();
		target.TakeDamage(weapon.damage);
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

	public void AimAt(Vector3 target) {
		StartCoroutine(AimAtCR(target));
	}

	public IEnumerator AimAtCR(Vector3 target) {
		float aimStep = 0;

		while(aimStep < 1f) {
			Vector3 targetDir = target - transform.position;
			aimStep += Time.deltaTime * aimSpeed;

			Vector3 newDir = Vector3.RotateTowards(transform.forward, -targetDir, aimStep, 0.0F);
			
			transform.rotation = Quaternion.LookRotation(newDir);
			yield return null;
		}
	}

	public void SetTargeted() {
		isTargeted = true;
		targetedIcon.SetActive(true);
	}

	public void ClearTargeted() {
		isTargeted = false;
		targetedIcon.SetActive(false);
	}
}
