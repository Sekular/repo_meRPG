using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

	public GameObject[] m_shieldObjects;			// List of objects that represent the shield visual in game.
	public float m_fShieldIntensity = 2.5f;			// Intensity of shield visual when initially struck.
	public float m_fFlashDecay = 2.5f;				// The speed at which the shield intensity decays to 0 after it is struck.
	public float m_fShieldBreakPulseSize = 1.5f;    // When the shield breaks, how large the mesh is pulsed for the effect.
	public float m_fShieldBreakEffectSpeed = 1f;	// When the shield breaks, how quickly the effect moves out and fades.
	public ParticleSystem m_ShieldBreakPFX;

	public void ShieldHit() {
		StopAllCoroutines();
		StartCoroutine(ShieldHitCR());
	}

	// Sets the Shield to its max intensity and then decays the visual over time.
	private IEnumerator ShieldHitCR() {
		foreach(GameObject so in m_shieldObjects) {
			SetShieldIntensity(so, m_fShieldIntensity);
		}

		float t = 0;
		float currentIntensity = 0;

		while (t < 1) {
			t += Time.deltaTime * m_fFlashDecay;
			currentIntensity = Mathf.Lerp(m_fShieldIntensity, 0f, t);
			foreach (GameObject so in m_shieldObjects) {
				SetShieldIntensity(so, currentIntensity);
				yield return null;
			}
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			ShieldBroken();
		}
	}

	public void ShieldBroken() {
		m_ShieldBreakPFX.Play();
	}

	private void SetShieldIntensity(GameObject so, float i) { so.GetComponent<Renderer>().material.SetFloat("_Strength", i); }
}
