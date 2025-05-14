using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    //定义一个面板可见的跑酷动作属性列表
    [SerializeField] List<ParkourAction> parkourActions;

    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;
    //是否在动作中
    bool inAction;


    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButton("Jump") && !inAction && playerController.isGrounded)
        {
            //调试用的射线也只会在if满足的时候触发
            //调用环境扫描器environment scanner的ObstacleCheck方法的返回值：ObstacleHitData结构体
            var hitData = environmentScanner.ObstacleCheck();
            if (hitData.forwardHitFound)
            {
                //对于每一个在跑酷动作列表中的跑酷动作
                foreach (var action in parkourActions)
                {
                    //如果动作可行
                    if(action.CheckIfPossible(hitData, transform))
                    {
                        //播放对应动画
                        //StartCoroutine()方法：开启一个协程
                        //启动 DoParkourAction 协程，播放跑酷动画
                        StartCoroutine(DoParkourAction(action));
                        //跳出循环
                        break;
                    }
                }
                //调试用：打印障碍物名称
                //Debug.Log("找到障碍：" + hitData.forwardHitInfo.transform.name);

            }
        }
    }
    //跑酷动作
    IEnumerator DoParkourAction(ParkourAction action)
    {
        //跑酷动作开始
        inAction = true;
        //禁用玩家控制
        playerController.SetControl(false);

        //设置动画是否镜像
        animator.SetBool("mirrorAction", action.Mirror);

        //从当前动画到指定的目标动画，平滑过渡0.2s
        animator.CrossFade(action.AnimName, 0.2f);

        // 等待过渡完成
        yield return new WaitForSeconds(0.3f); // 给足够时间让过渡完成，稍微大于CrossFade的过渡时间

        // 现在获取动画状态信息
        var animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        //#region 调试用
        //if (!animStateInfo.IsName(action.AnimName))
        //{
        //    Debug.LogError("动画名称不匹配！");
        //}
        //#endregion

        ////暂停协程，直到 "StepUp" 动画播放完毕。
        //yield return new WaitForSeconds(animStateInfo.length);

        //动画播放期间，暂停协程，并让角色平滑旋转向障碍物
        float timer = 0f;
        while (timer <= animStateInfo.length)
        {
            timer += Time.deltaTime;
            //如果勾选该动作需要旋转向障碍物RotateToObstacle
            if (action.RotateToObstacle)
            {
                //让角色平滑旋转向障碍物
                transform.rotation = Quaternion.RotateTowards(transform.rotation,action.TargetRotation, 
                                                        playerController.RotationSpeed * Time.deltaTime);
            }
            //如果勾选目标匹配EnableTargetMatching
            //只有当不在过渡状态时才执行目标匹配
            if (action.EnableTargetMatching && !animator.IsInTransition(0))
            {
                MatchTarget(action);
            }

            //过渡动画完全播完就停止该动作播放
            if(animator.IsInTransition(0) && timer > 0.5f){
                break;
            }

            yield return null;
        }
        //对于一些组合动作，第一阶段播放完后就会被输入控制打断，这时候给一个延迟，让第二阶段的动画也播放完
        //对于ClimbUp动作，第二阶段就是CrouchToStand
        yield return new WaitForSeconds(action.ActionDelay);
        //延迟结束后才启用玩家控制
        playerController.SetControl(true);
        //跑酷动作结束
        inAction = false;
    }

    //目标匹配
    void MatchTarget(ParkourAction action)
    {
        //只有在不匹配和不在过渡状态的时候才会调用
        if (animator.isMatchingTarget || animator.IsInTransition(0))
        {
            return;
        }
        //调用unity自带的MatchTarget方法
        animator.MatchTarget(action.MatchPosition, transform.rotation, action.MatchBodyPart, 
                        new MatchTargetWeightMask(action.MatchPositionXYZWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }

}
