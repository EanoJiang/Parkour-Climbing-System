//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerJumpController : MonoBehaviour
//{
//    //ÊÇ·ñÌøÔ¾
//    public bool jump;
//    bool lastJump;
//    public bool lockPlannar;

//    Animator animator; 
//    PlayerController playerController;

//    private void Awake()
//    {
//        animator = GetComponent<Animator>();
//        playerController = GetComponent<PlayerController>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        #region Jump
//        bool newJump = Input.GetButton("Jump");
//        if (lastJump == false && newJump == true)
//        {
//            jump = true;
//            //Debug.Log("jump trigger");
//        }
//        else
//        {
//            jump = false;
//        }
//        lastJump = newJump;
//        if (jump)
//        {
//            animator.SetTrigger("jump");
//        }
//        #endregion
//    }

//    public void OnJumpEnter(){
//        Debug.Log("ÆðÌø");
//        playerController.inputEnabled = false;
//        lockPlannar = true;
//    }
//    public void OnJumpExit(){
//        Debug.Log("ÂäµØ"); 
//        playerController.inputEnabled = true ;
//        lockPlannar = false;
//    }
//}
