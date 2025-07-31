using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonRoll : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }


    public void OnRoll(InputValue value)
    {
        if(value.isPressed)
            animator.SetTrigger("roll");

    }
}
