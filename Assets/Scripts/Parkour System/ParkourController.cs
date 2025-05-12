using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    EnvironmentScanner environmentScanner;
    Animator animator;
    //是否在动作中
    bool inAction;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButton("Jump") && !inAction)
        {
            //调用环境扫描器environment scanner的ObstacleCheck方法的返回值：ObstacleHitData结构体
            var hitData = environmentScanner.ObstacleCheck();
            if (hitData.forwardHitFound)
            {
                //调试用：打印障碍物名称
                Debug.Log("找到障碍：" + hitData.forwardHitInfo.transform.name);
                //播放动画
                //StartCoroutine()方法：开启一个协程
                //启动 DoParkourAction 协程，播放攀爬动画
                StartCoroutine(DoParkourAction());

            }
        }
    }
    //攀爬动作
    IEnumerator DoParkourAction(){
        inAction = true;

        //从当前动画到StepUp动画，平滑过渡0.2s
        //CrossFade()方法：平滑地从当前动画过渡到指定的目标动画
        animator.CrossFade("StepUp", 0.2f);
        //暂停协程，直到下一帧继续执行，确保动画过渡已经开始。
        yield return null;

        //第0层动画，也就是StepUp，用来后面调用这个动画的长度等属性
        var animStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //暂停协程，直到 "StepUp" 动画播放完毕。
        yield return new WaitForSeconds(animStateInfo.length);

        inAction = false;
    }

}
