using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public float m_fRange;		// The range of the weapon. Used to determine hit chance & if the Actor is in range for an attack.
	public int m_iDamage;		// The damage the weapon deals when it strikes an Actor.
	public ParticleSystem m_psFiring;		// The projectile (bullet) effect played when the weapon is fired.
    public ParticleSystem m_psFlashPS;		// The muzzle flash effect played when the weapon is fired.
    public int maxHeat;		// (Shots before reload) The max heat the weapon can contain before requiring a replacement heat sink.
    [HideInInspector] public int currentHeat; // The current heat of the weapon.

    public void Fire() {
		currentHeat++;

		m_psFiring.Play();
		m_psFlashPS.Play();
    }
}
