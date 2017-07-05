using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public float range;
	public int damage;

	public ParticleSystem firingPS;
    public ParticleSystem flashPS;

    public int heatLimit;
    [HideInInspector] public int currentHeat;

    public void Fire() {
		currentHeat++;

		firingPS.Play();
        flashPS.Play();
    }
}
