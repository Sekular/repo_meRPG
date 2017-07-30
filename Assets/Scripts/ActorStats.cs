using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorStats : MonoBehaviour {

	public Canvas m_statCanvas;   // The canvas that contains all stat UI elements.
	public RectTransform m_coverIcon, m_shieldBar, m_healthBar, m_shieldPip, m_healthPip, m_hitChance;   // UI elements contained in the StatCanvas.
	public Vector2 m_vCoverIconOffset = new Vector2(18.5f, 23.5f);    // Offset for placing the Cover Icon.
	public Vector2 m_vShieldBarOffset = new Vector2(1f, 40f);   // Offset for placing the Shield Bar.
	public Vector2 m_vHealthBarOffset = new Vector2(1f, 40f);   // Offset for placing the Health Bar.
	public Vector2 m_vHitChanceOffset = new Vector2(1f, 40f);   // Offset for placing the Hit Chance text.
	public float m_fBarWidth = 75f;   // How wide Health & Shield bars should be when they're full.

	private Actor actor = null;   // The Actor that this ActorStats belongs to.
	private float healthPipWidth = 5f, shieldPipWidth = 5f, pipHeight = 5f;   // Used to dynamically set the width/height of each health/shield pip to fit on their respective bar.
	private Vector2 shieldOrigin = new Vector2(60, 50), healthOrigin = new Vector2(60, 45);   // Origin points for placing the shield/health pips along their respective bar.
	private List<RectTransform> healthPips = new List<RectTransform>(), shieldPips = new List<RectTransform>();    // Lists of the current health/shield pips. Used to display health/damage.

	public void Deactivate() { m_statCanvas.gameObject.SetActive(false); }

	void Awake() {
		actor = this.GetComponent<Actor>();
		healthPipWidth = m_fBarWidth / actor.maxHealth;
		shieldPipWidth = m_fBarWidth / actor.maxShield;
		GenerateStatBars();
	}

	void GenerateStatBars() {
		for (int i = 0; i < actor.maxShield; i++) {
			RectTransform temp = Instantiate(m_shieldPip, transform.position, Quaternion.identity);
			shieldPips.Add(temp);
			temp.SetParent(m_shieldBar.transform);
			temp.sizeDelta = new Vector2(shieldPipWidth, pipHeight);
			temp.localScale = Vector3.one;
			temp.anchoredPosition = shieldOrigin + new Vector2((shieldPipWidth * i), 0f);
		}

		for (int i = 0; i < actor.maxHealth; i++) {
			RectTransform temp = Instantiate(m_healthPip, transform.position, Quaternion.identity);
			healthPips.Add(temp);
			temp.SetParent(m_healthBar.transform);
			temp.sizeDelta = new Vector2(healthPipWidth, pipHeight);
			temp.localScale = Vector3.one;
			temp.anchoredPosition = healthOrigin + new Vector2((healthPipWidth * i), 0f);
		}
	}

	public void UpdateShieldHealthVisuals() {
		for (int i = 0; i < healthPips.Count; i++) {
			if(i + 1 <= actor.currentHealth) {
				healthPips[i].gameObject.SetActive(true);
			}
			else {
				healthPips[i].gameObject.SetActive(false);
			}
		}

		for (int i = 0; i < shieldPips.Count; i++) {
			if (i + 1 <= actor.currentShield) {
				shieldPips[i].gameObject.SetActive(true);
			}
			else {
				shieldPips[i].gameObject.SetActive(false);
			}
		}
	}

	void Update() {
		PositionStatElements();
	}

	// Used to calculate and position the UI elements in screen space based on the Actors current location on screen.
	void PositionStatElements()	{
		RectTransform CanvasRect = m_statCanvas.GetComponent<RectTransform>();

		Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(transform.position);
		Vector2 WorldObject_ScreenPosition = new Vector2(
		((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
		((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

		Vector2 coverIconPos = WorldObject_ScreenPosition + m_vCoverIconOffset;
		Vector2 shieldBarPos = WorldObject_ScreenPosition + m_vShieldBarOffset;
		Vector2 healthBarPos = WorldObject_ScreenPosition + m_vHealthBarOffset;
		Vector2 hitChancePos = WorldObject_ScreenPosition + m_vHitChanceOffset;

		m_coverIcon.anchoredPosition = coverIconPos;
		m_shieldBar.anchoredPosition = shieldBarPos;
		m_healthBar.anchoredPosition = healthBarPos;
		m_hitChance.anchoredPosition = hitChancePos;
	}
}
