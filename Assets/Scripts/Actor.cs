﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour {
	//___VISUALS________________________________________________//
	private Animator anim;
	public Sprite characterPortrait;

	//___MOVEMENT_______________________________________________//
	[HideInInspector] public int tileX, tileZ;
	[HideInInspector] public Grid grid;
	[HideInInspector] public List<Node> currentPath = null;
	private Vector3 lerpStart;
	float moveStep = 0;
	public float rotateSpeed;

	[HideInInspector] public bool isMoving;

	public float moveDist = 5;
	public float moveSpeed = 1;
	public float aimSpeed = 1;

	//___COMBAT_________________________________________________//
	private CombatManager combatManager;
	[HideInInspector] public Weapon weapon;

	public float aimDelay;
	public float fireTime;

	public int maxHealth;
	[HideInInspector] public int currentHealth;
	public int maxShield;
	[HideInInspector] public int currentShield;

	//___MANAGEMENT_ ___________________________________________//
	[HideInInspector] public bool isAwaitingOrders = false;
	[HideInInspector] public bool hasActed = false;
	[HideInInspector] public bool hasMoved = false;
	[HideInInspector] public bool isIncap = false;
	[HideInInspector] public int team = 0;

	public GameObject availableIcon;
	public GameObject selectedIcon;
	public GameObject targetedIcon;
	public GameObject reloadIcon;

	[HideInInspector] public bool isTargeted;

	Vector3 targetDir = Vector3.zero;
	Vector3 weaponDir = Vector3.zero;
	Vector3 finalFacing = Vector3.zero;
	Vector3 a, b, c;
	Vector3 newDir = Vector3.zero;


	//___HELPERS_ _____________________________________________//
	public void SetAvailable() { availableIcon.SetActive(true); }
	public void SetUnavailable() { availableIcon.SetActive(false); }
	public void Selected() { selectedIcon.SetActive(true); }
	public void ReloadActive() { reloadIcon.SetActive(true); }
	public void ReloadDeactive() { reloadIcon.SetActive(false); }
	public void Deselected() { selectedIcon.SetActive(false); }
	public void Deactivate() { availableIcon.SetActive(false); selectedIcon.SetActive(false); targetedIcon.SetActive(false); reloadIcon.SetActive(false); }

	void Awake() {
		SetKinematic(true);
		anim = GetComponentInChildren<Animator>();
		weapon = GetComponentInChildren<Weapon>();
		combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
		grid = GameObject.Find("Grid").GetComponent<Grid>();

		currentHealth = maxHealth;
		currentShield = maxShield;
	}

	void Update() {
		Debug.DrawLine(transform.position, a, Color.red);
		Debug.DrawLine(transform.position, b, Color.blue);
		Debug.DrawLine(transform.position, finalFacing, Color.green);
		Debug.DrawLine(transform.position, transform.forward + transform.position, Color.yellow);
	}

	public void ResetActions() {
		hasActed = false;
		hasMoved = false;
	}

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

	public void Move(int x, int z) {
		combatManager.DeactivateTeammates(team, this);
		isMoving = true;
		anim.SetBool("IsMoving", true);
		grid.GeneratePathTo(x, z);

		tileX = currentPath[1].x;
		tileZ = currentPath[1].z;
		lerpStart = transform.position;

		StartCoroutine(MoveCR());
	}

	IEnumerator MoveCR() {
		MoveAt(grid.TileToWorld(tileX, tileZ));

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

	// Advances path-finding progress by one node. Used for movement.
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
		MoveAt(grid.TileToWorld(tileX, tileZ));

		// Remove the old "current" tile from the path-finding list
		currentPath.RemoveAt(0);
	}

	public void Attack(Actor target, Actor attacker)
    {
        StartCoroutine(AttackCR(target, attacker));
    }

    public IEnumerator AttackCR(Actor target, Actor attacker)
    {
		AimAt(target.transform.position);

		anim.SetBool("IsAiming", true);
        yield return new WaitForSeconds(aimDelay);
        weapon.Fire();
		yield return new WaitForSeconds(fireTime);
        anim.SetBool("IsAiming", false);
		target.TakeDamage(weapon.damage, attacker);

        hasActed = true;

        combatManager.StartNextTurn();
    }

	public void Reload() {
		weapon.currentHeat = 0;
		hasActed = true;

		combatManager.StartNextTurn();
	}

	public void TakeDamage(int damage, Actor attacker)
	{
		currentShield -= damage;

		if(currentShield < 0) {
			currentHealth += currentShield;
			currentShield = 0;
		}

		if(currentHealth <= 0)
		{
			isIncap = true;

			SetKinematic(false);
			weapon.transform.parent = null;
			GetComponentInChildren<Animator>().enabled = false;

			Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();

			Vector3 hitVector = (transform.position - attacker.transform.position);


			rigidBodies[0].AddForce(hitVector.x * 10f, hitVector.y * 10f, hitVector.z * 10f, ForceMode.Impulse);

			Debug.Log(gameObject.name + " has been incapacitated.");

			// TODO Play death animation.
		}
	}

	public void MoveAt(Vector3 target) {
		StartCoroutine(MoveAtCR(target));
	}

	public IEnumerator MoveAtCR(Vector3 target) {
		float aimStep = 0;

		while(aimStep < 1f) {
			Vector3 targetDir = target - transform.position;
			aimStep += Time.deltaTime * rotateSpeed;

			Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, aimStep, 0.0F);
			
			transform.rotation = Quaternion.LookRotation(newDir);
			yield return null;
		}
	}

	public void AimAt(Vector3 target) {
		StartCoroutine(AimAtCR(target));
	}

	public IEnumerator AimAtCR(Vector3 target) {
		float aimStep = 0;

		while (aimStep < 1f) {
			targetDir = (target - transform.position);

			aimStep += Time.deltaTime * aimSpeed;

			newDir = Vector3.Lerp(transform.forward, targetDir, aimStep);
			transform.rotation = Quaternion.LookRotation(newDir);
			yield return null;
		}

		a = transform.position + targetDir;
		a.y = 0f;
		b = (weapon.transform.forward * targetDir.magnitude) + transform.position;
		b.y = 0f;
		c = a - b;
		finalFacing = a + c;
		aimStep = 0f;

		transform.LookAt(finalFacing);

		/*targetDir = (target - transform.position);
		a = transform.position + targetDir;
		a.y = 0f;
		b = (weapon.transform.forward * targetDir.magnitude) + transform.position;
		b.y = 0f;
		c = a - b;
		finalFacing = a + c;
		aimStep = 0f;

		transform.LookAt(finalFacing);*/
	}

	public void SetTargeted() {
		isTargeted = true;
		targetedIcon.SetActive(true);
	}

	public void ClearTargeted() {
		isTargeted = false;
		targetedIcon.SetActive(false);
	}

	void SetKinematic(bool newValue) {
		Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rb in bodies) {
			rb.isKinematic = newValue;
		}
	}
}
