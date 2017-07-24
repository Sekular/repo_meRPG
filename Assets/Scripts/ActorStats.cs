using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorStats : MonoBehaviour {

	public RectTransform canvasRectT;
	public RectTransform coverIcon;
	public Vector2 coverIconOffset;
	public RectTransform shieldBar;
	public Vector2 shieldBarOffset;
	public RectTransform healthBar;
	public Vector2 healthBarOffset;
	public Canvas canvas;
	public float barWidth = 75f;
	public RectTransform m_shieldPip = null;
	public RectTransform m_healthPip = null;

	private Actor actor = null;
	private float healthPipWidth = 0f;
	private float shieldPipWidth = 0f;
	private float pipHeight = 5f;
	private Vector2 shieldOrigin = new Vector2(60, 50);
	private Vector2 healthOrigin = new Vector2(60, 45);
	public List<RectTransform> healthPips, shieldPips = null;

	public void Deactivate() { canvas.gameObject.SetActive(false); }

	void Awake() {
		actor = this.GetComponent<Actor>();
		healthPipWidth = barWidth / actor.maxHealth;
		shieldPipWidth = barWidth / actor.maxShield;
		GenerateStatBars();
	}

	void GenerateStatBars() {
		for (int i = 0; i < actor.maxShield; i++) {
			RectTransform temp = Instantiate(m_shieldPip, transform.position, Quaternion.identity);
			shieldPips.Add(temp);
			temp.SetParent(shieldBar.transform);
			temp.sizeDelta = new Vector2(shieldPipWidth, pipHeight);
			temp.localScale = Vector3.one;
			temp.anchoredPosition = shieldOrigin + new Vector2((shieldPipWidth * i), 0f);
		}

		for (int i = 0; i < actor.maxHealth; i++) {
			RectTransform temp = Instantiate(m_healthPip, transform.position, Quaternion.identity);
			healthPips.Add(temp);
			temp.SetParent(healthBar.transform);
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

		//first you need the RectTransform component of your canvas
		RectTransform CanvasRect = canvas.GetComponent<RectTransform>();

		//then you calculate the position of the UI element
		//0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.

		Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(transform.position);
		Vector2 WorldObject_ScreenPosition = new Vector2(
		((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
		((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

		Vector2 coverIconPos = WorldObject_ScreenPosition + coverIconOffset;
		Vector2 shieldBarPos = WorldObject_ScreenPosition + shieldBarOffset;
		Vector2 healthBarPos = WorldObject_ScreenPosition + healthBarOffset;

		//now you can set the position of the ui element
		coverIcon.anchoredPosition = coverIconPos;
		shieldBar.anchoredPosition = shieldBarPos;
		healthBar.anchoredPosition = healthBarPos;
	}
}
