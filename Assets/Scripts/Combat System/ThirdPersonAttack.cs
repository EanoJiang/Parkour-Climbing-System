using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonAttack : MonoBehaviour
{
    [Header("轻击动画列表")]
    [SerializeField] private AnimationClip[] Attack1Clips;
    [Header("重击动画列表")]
    [SerializeField] private AnimationClip[] Attack2Clips;

    PlayerController playerController;
    Animator animator;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    //攻击类输入存储值，会被新的输入值覆盖
    int inputAttackType = 0;
    
    //轻击
    void OnFire1(InputValue value)
    {
        if (value.isPressed)
        {
            inputAttackType = 1;

        }
    }

    //重击
    void OnFire2(InputValue value)
    {
        if (value.isPressed)
        {
            inputAttackType = 2;
        }
    }

    //连招数
    public int currentAttack = 0;
    //当前攻击的动画时长计时器
    float animTimer = 0;
    /// <summary>
    /// 攻击动画加载逻辑
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputAttackType"></param>
    void PlayerAttack(int index,int inputAttackType)
    {
        //攻击时取消输入
        playerController.inputEnabled = false;
        AnimationClip animationclip;
        if (inputAttackType == 1)
        {
            //轻击
            //动画载入
            animationclip = Attack1Clips[index];
            //连招数加1
            currentAttack++;
        }
        else
        {
            //重击
            //动画载入
            animationclip = Attack2Clips[index];
            //连招数归零
            currentAttack = 0;
        }
        animator.CrossFade(animationclip.name, 0.2f);
        animTimer = animationclip.length;
    }

    void Update()
    {
        if (playerController.IsHanging || !playerController.IsGrounded)
            return;
        //动画时长计时器帧自减
        animTimer -= Time.deltaTime;
        if(animTimer <= 0)
        {
            //动画播放完毕
            //恢复输入
            playerController.inputEnabled = true;
            //连招数归零
            currentAttack = 0;
        }
        //预输入逻辑
        if(inputAttackType != 0)
        {
            //如果有攻击键输入
            //在当前攻击动画播放结束前0.4s，(并且当前的连招数<攻击动画列表的长度)
            //播放对应索引的攻击动画
            if(animTimer <= 0.4f && currentAttack < Attack1Clips.Length)
            {
                PlayerAttack(currentAttack, inputAttackType);
            }
            //攻击类输入存储值归零
            inputAttackType = 0;
        }

    }

    //外部调用的属性
    public int InputAttackType => inputAttackType;

}
