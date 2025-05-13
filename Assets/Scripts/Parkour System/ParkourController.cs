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
        if (Input.GetButton("Jump") && !inAction)
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

        //从当前动画到指定的目标动画，平滑过渡0.2s
        //CrossFade()方法：平滑地从当前动画过渡到指定的目标动画
        animator.CrossFade(action.AnimName, 0.2f);
        ////暂停协程，直到下一帧继续执行，确保动画过渡已经开始。
        yield return null;

        //第0层动画，也就是StepUp，用来后面调用这个动画的长度等属性
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
            if (action.EnableTargetMatching)
            {
                MatchTarget(action);
            }

            yield return null;
        }


        //启用玩家控制
        playerController.SetControl(true);
        //跑酷动作结束
        inAction = false;
    }

    void MatchTarget(ParkourAction action)
    {
        //只有在不匹配的时候才会调用
        if (animator.isMatchingTarget)
        {
            return;
        }
        //调用unity自带的MatchTarget方法
        animator.MatchTarget(action.MatchPosition, transform.rotation, action.MatchBodyPart, 
                        new MatchTargetWeightMask(new Vector3(0, 1, 0), 0), action.MatchStartTime, action.MatchTargetTime);
    }

}
