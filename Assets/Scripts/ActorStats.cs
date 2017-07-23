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

	public void Deactivate() { canvas.gameObject.SetActive(false); }

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
