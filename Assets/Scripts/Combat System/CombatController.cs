using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{   
    MeleeFighter meleeFighter;
    PlayerController playerController;
    //PlayerJumpController playerJumpController;
    private void Awake()
    {
        meleeFighter = GetComponent<MeleeFighter>();
        playerController = GetComponent<PlayerController>();
        //playerJumpController = GetComponent<PlayerJumpController>();
    }
    private void Update()
    {
        if (playerController.IsHanging || !playerController.IsGrounded)
            return;
        if (Input.GetButtonDown("Attack"))
        {
            meleeFighter.TryToAttack();
        }
    }
}
