using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

	public GameObject[] shieldObjects;

	public float shieldIntensity;
	public float decayDelay;
	public float flashDecay;

	public void ShieldHit() {
		StopAllCoroutines();
		StartCoroutine(ShieldHitCR());
	}

	public void ShieldBroken() {
		Debug.Log("Play Shield Broken Effect");
	}

	private IEnumerator ShieldHitCR() {
		foreach(GameObject so in shieldObjects) {
			Renderer r = so.GetComponent<Renderer>();
			r.material.SetFloat("_Strength", shieldIntensity);
		}

		//yield return new WaitForSeconds(decayDelay);

		float t = 0;
		float currentIntensity = 0;

		while (t < 1) {
			t += Time.deltaTime * flashDecay;
			currentIntensity = Mathf.Lerp(shieldIntensity, 0f, t);
			foreach (GameObject so in shieldObjects) {
				Renderer r = so.GetComponent<Renderer>();
				r.material.SetFloat("_Strength", currentIntensity);
				yield return null;
			}
		}
	}
	
}
