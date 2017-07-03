using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public ParticleSystem firingPS;
    public ParticleSystem flashPS;

    public int heatLimit;
    [HideInInspector] public int currentHeat;
    	
	void Update ()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }*/
	}

    public void Fire()
    {
		currentHeat++;

		firingPS.Play();
        flashPS.Play();
    }
}
