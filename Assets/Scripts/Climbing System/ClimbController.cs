using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [SerializeField] public MatchTimeParams idleToHang;//0.4~0.6    0.25,0.15,0.15
    [SerializeField] public MatchTimeParams HangHopUp;//0.34~0.65   0.25,0.18,0.15
    [SerializeField] public MatchTimeParams HangHopDown;//0.31~0.7  0.25,0.09,0.12
    [SerializeField] public MatchTimeParams HangHopRight;//0.2~0.8  0.25,0.19,0.09
    [SerializeField] public MatchTimeParams ShimmyRight;//0~0.38    0.25,0.18,0.12

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
                    //currentPoint = 击中点对象的组件ClimbPoint
                    currentPoint = ledgeHit.transform.GetComponent<ClimbPoint>();
                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", ledgeHit.transform, idleToHang.matchStartTime, idleToHang.matchTargetTime));
                }
            }
            #endregion
        }
        else
        {
            #region Ledge To Ledge
            
            //Mathf.Round(...)：对输入值四舍五入，确保结果为 +-1 / 0。
            float h = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float v = Mathf.Round(Input.GetAxisRaw("Vertical"));
            var inputDir = new Vector2(h, v);
            
            if (playerController.InAction || inputDir == Vector2.zero)
                return;

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
            matchPosition = getHandPos(ledge,hand,handOffset),
            matchBodyPart = hand,
            matchPositionXYZWeight = new Vector3(1, 1, 1),
            matchStartTime = matchStartTime,
            matchTargetTime = matchTargetTime
        };
        var targetRotation = Quaternion.LookRotation(-ledge.forward);
        yield return playerController.DoAction(anim, matchParams, targetRotation, true);
        playerController.IsHanging = true;
    }

    private Vector3 getHandPos(Transform ledge,AvatarTarget hand, Vector3? handOffset) {
        var offsetValue = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.17f, 0.14f);
        var handDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + Vector3.up * offsetValue.y + ledge.forward * offsetValue.z - handDir * offsetValue.x; //Ledge的左边也就是人物的右边
    }
}

[System.Serializable]
public struct MatchTimeParams
{
    public float matchStartTime;
    public float matchTargetTime;
    public Vector3 handOffset;
}