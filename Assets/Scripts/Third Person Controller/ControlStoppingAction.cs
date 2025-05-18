using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlStoppingAction : StateMachineBehaviour
{
    PlayerController player;

    //进入该状态时调用
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //如果player还没有被赋值，则获取player组件
        if (player == null)
        {
            player = animator.GetComponent<PlayerController>();
        }
        //禁用玩家的控制
        player.HasControl = false;

    }
    //退出该状态时调用
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //恢复玩家的控制
        animator.GetComponent<PlayerController>().HasControl = true;
    }
}
