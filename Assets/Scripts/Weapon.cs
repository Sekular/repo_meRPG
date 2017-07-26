using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public float m_fRange;
	public int m_iDamage;
	public ParticleSystem m_psFiring;
    public ParticleSystem m_psFlashPS;
    public int maxHeat;
    [HideInInspector] public int currentHeat;

    public void Fire() {
		currentHeat++;

		m_psFiring.Play();
		m_psFlashPS.Play();
    }
}
