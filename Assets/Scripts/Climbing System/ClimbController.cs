using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] public MatchTimeParams idleToHang;//0.4~0.6    0.25,0.15,0.15
    [SerializeField] public MatchTimeParams DropToHang;//0.25~0.5   0.25,0.15,-0.05
    [SerializeField] public MatchTimeParams HangHopUp;//0.34~0.65   0.25,0.18,0.15
    [SerializeField] public MatchTimeParams HangHopDown;//0.31~0.7  0.25,0.09,0.12
    [SerializeField] public MatchTimeParams HangHopRight;//0.2~0.8  0.25,0.19,0.09
    [SerializeField] public MatchTimeParams ShimmyRight;//0~0.38    0.25,0.18,0.06

    ClimbPoint currentPoint;
    EnvironmentScanner envScanner;
    PlayerController playerController;
    public bool IsOnClimbLedge { get; private set; }
    void Awake()
    {
        envScanner = GetComponent<EnvironmentScanner>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!playerController.IsHanging)
        {
            #region IdleToHang
            if (Input.GetButton("Jump") && !playerController.InAction)  //其他动作不在播放时
            {
                IsOnClimbLedge = envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit);
                if (IsOnClimbLedge)
                {
                    //currentPoint = 离射线击中点最近的ClimbPoint
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);//击中点的物体本身(也就是对应的边沿攀岩架)，和击中点
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, idleToHang.matchStartTime, idleToHang.matchTargetTime));
                }
            }
            #endregion

            #region Drop to Hang
            //这里发现动画有问题，转身不是完全180度转身，差大概30度，所以在JumpToLedge()中我对这个动画DropToHang加了一个旋转补偿
            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                //需要一个检测边沿攀岩架的方法
                bool isOnDropLedge = envScanner.DropLedgeCheck(out RaycastHit ledgeHit);
                if (isOnDropLedge)
                {
                    //currentPoint = 离射线击中点最近的ClimbPoint
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);//击中点的物体本身(也就是对应的边沿攀岩架)，和击中点
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, DropToHang.matchStartTime, DropToHang.matchTargetTime, handOffset: DropToHang.handOffset));
                }
            }
            #endregion
        }
        else
        {
            #region Jump from Hang
            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                playerController.IsHanging = false;
                StartCoroutine(JumpFromHang());
                return;
            }
            #endregion

            #region Ledge To Ledge

            //Mathf.Round(...)：对输入值四舍五入，确保结果为 +-1 / 0。
            float h = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float v = Mathf.Round(Input.GetAxisRaw("Vertical"));
            var inputDir = new Vector2(h, v);


            if (playerController.InAction || inputDir == Vector2.zero)
                return;

            //从Hang状态爬上Ledge
            if (currentPoint.MountPoint && inputDir.y == 1)
            {
                playerController.IsHanging = false;
                StartCoroutine(MountFromHang());
                return;
            }

            var neighbour = currentPoint.GetNeighbour(inputDir);

            if (neighbour == null)
                return;
            if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                //更新currentPoint为邻居攀岩架的point
                currentPoint = neighbour.point;
                if (neighbour.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, HangHopUp.matchStartTime, HangHopUp.matchTargetTime, handOffset: HangHopUp.handOffset));
                else if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, HangHopDown.matchStartTime, HangHopDown.matchTargetTime, handOffset: HangHopDown.handOffset));
                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, HangHopRight.matchStartTime, HangHopRight.matchTargetTime, handOffset: HangHopRight.handOffset));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, HangHopRight.matchStartTime, HangHopRight.matchTargetTime, handOffset: HangHopRight.handOffset));
            }
            else if (neighbour.connectionType == ConnectionType.Move)
            {
                //更新currentPoint为邻居攀岩架的point
                currentPoint = neighbour.point;
                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, ShimmyRight.matchStartTime, ShimmyRight.matchTargetTime, handOffset: ShimmyRight.handOffset));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, ShimmyRight.matchStartTime, ShimmyRight.matchTargetTime, AvatarTarget.LeftHand, handOffset: ShimmyRight.handOffset));
            }

            #endregion

        }
    }

    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime,
                        AvatarTarget hand = AvatarTarget.RightHand,
                        Vector3? handOffset = null)
    {
        var matchParams = new MatchTargetParams()
        {
            matchPosition = getHandPos(ledge, hand, handOffset),
            matchBodyPart = hand,
            matchPositionXYZWeight = new Vector3(1, 1, 1),
            matchStartTime = matchStartTime,
            matchTargetTime = matchTargetTime
        };

        // 基础旋转：面向攀岩架
        var targetRotation = Quaternion.LookRotation(-ledge.forward);
        
        // 根据不同的动画类型添加额外的旋转补偿
        if (anim == "DropToHang")
        {
            // 添加180度旋转来补偿动画中的手部位置偏差
            targetRotation *= Quaternion.Euler(0, 30, 0);
        }
        
        yield return playerController.DoAction(anim, matchParams, targetRotation, true);
        playerController.IsHanging = true;
    }

    /// <summary>
    /// 获取动作匹配用到的手部位置偏移
    /// </summary>
    /// <param name="ledge"></param>
    /// <param name="hand"></param>
    /// <param name="handOffset"></param>
    /// <returns></returns>
    private Vector3 getHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset)
    {
        var offsetValue = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.17f, 0.14f);
        var handDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + Vector3.up * offsetValue.y + ledge.forward * offsetValue.z - handDir * offsetValue.x; //Ledge的左边也就是人物的右边
    }

    IEnumerator JumpFromHang()
    {
        yield return playerController.DoAction("JumpFromHang");
        playerController.ResetRotation();
        playerController.SetControl(true);
    }

    IEnumerator MountFromHang()
    {
        yield return playerController.DoAction("ClimbFromHang");
        //临时启用角色物理控制器而不启用输入控制
        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);
        playerController.ResetRotation();
        //启用角色控制和输入控制
        playerController.SetControl(true);
    }

    /// <summary>
    /// 从当前攀岩架上的每个点找到离射线击中点最近的挂点
    /// </summary>
    /// <param name="ledge"></param>攀岩架对象
    /// <param name="hitPoint"></param>射线击中攀岩架的点的位置
    /// <returns></returns>
    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        //获取边沿攀岩架的所有子对象的ClimbPoint节点 数组points
        var points = ledge.GetComponentsInChildren<ClimbPoint>();
        ClimbPoint nearestPoint = null;
        float minDistance = Mathf.Infinity;
        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }
        return nearestPoint;
    }

}

[System.Serializable]
public struct MatchTimeParams
{
    public float matchStartTime;
    public float matchTargetTime;
    public Vector3 handOffset;
}