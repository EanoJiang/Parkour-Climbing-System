using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    //定义一个面板可见的跑酷动作属性列表
    [Header("跑酷动作列表")]
    [SerializeField] List<ParkourAction> parkourActions;
    [Header("跳下悬崖动画")]
    [SerializeField] ParkourAction jumpDownAction;
    [Header("自动跳下高度")]
    [SerializeField] float autoJumpDownHeight = 1f;

    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;

    bool shouldJump;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    // Update is called once per frame
    private void Update()
    {
        //调用环境扫描器environment scanner的ObstacleCheck方法的返回值：ObstacleHitData结构体
        var hitData = environmentScanner.ObstacleCheck();
        #region 各种跑酷动作
        if (Input.GetButton("Jump") && !playerController.InAction && !playerController.IsHanging)
        {
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
        #endregion

        #region 悬崖跳下动作
        //在悬崖边沿且不在播放动作中且前方没有障碍物
        if (playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
        {
            //低矮的落差shouldJump == true，直接播放JumpDown动画
            shouldJump = true;
            //只有高度大于autoJumpHeight 且 玩家按下Drop键才会跳下悬崖
            if (playerController.LedgeHitData.height > autoJumpDownHeight && !Input.GetButtonDown("Drop")){
                shouldJump = false;
            }
            //偏差角度小于50度，才会播放JumpDown动画
            if(playerController.LedgeHitData.angle <= 50 && shouldJump){
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));
            }
        }
        #endregion
    }
    
    //跑酷动作
    IEnumerator DoParkourAction(ParkourAction action)
    {
        //禁用玩家控制
        playerController.SetControl(false);

        MatchTargetParams matchParams = null;
        if(action.EnableTargetMatching){
            if(matchParams == null){
                matchParams = new MatchTargetParams(){
                    matchPosition = action.MatchPosition,
                    matchBodyPart = action.MatchBodyPart,
                    matchPositionXYZWeight = action.MatchPositionXYZWeight,
                    matchStartTime = action.MatchStartTime,
                    matchTargetTime = action.MatchTargetTime
                };
            }
        }

        yield return playerController.DoAction(action.AnimName, matchParams, transform.rotation, 
                                         action.RotateToObstacle, action.ActionDelay, action.Mirror);

        //延迟结束后才启用玩家控制
        playerController.SetControl(true);       
    }

    //外部可访问的属性
    public bool ShouldJumpDown => shouldJump;
}
