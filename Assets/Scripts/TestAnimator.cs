using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

   
    void Update ()
    {
        RunInputs();
	}

    void RunInputs()
    {
        if(Input.GetKey(KeyCode.A))
        {
            SetAiming();
        }
        if (Input.GetKey(KeyCode.M))
        {
            SetMoving();
        }
        if (Input.GetKey(KeyCode.F))
        {
            SetFiring();
        }
    }

    void SetAiming()
    {
        anim.SetBool("IsAiming", true);
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsFiring", false);
    }

    void SetMoving()
    {
        anim.SetBool("IsAiming", false);
        anim.SetBool("IsMoving", true);
        anim.SetBool("IsFiring", false);
    }

    void SetFiring()
    {
        anim.SetBool("IsAiming", false);
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsFiring", true);
    }
}
