using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Parkour Action")]
public class ParkourAction : ScriptableObject
{
    //动画名称
    [SerializeField] string animName;
    //高度区间
    [SerializeField] float minHeigth;
    [SerializeField] float maxHeigth;

    [Header ("自主勾选该动作是否需要转向障碍物")]
    [SerializeField] bool rotateToObstacle;

    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching;
    [SerializeField] AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;


    //目标旋转量
    public Quaternion TargetRotation { get; set; }
    //匹配的位置
    public Vector3 MatchPosition { get; set; }

    public bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        //获取面前的障碍物高度 = 击中点上方一定高度的y轴坐标 - 玩家的y轴坐标
        float height = hitData.heightHitInfo.point.y - player.position.y;
        //只有在这个区间内才会返回true
        if(height < minHeigth || height > maxHeigth)
        {
            return false;
        }

        //如果需要转向障碍物，才会计算目标旋转量
        if (rotateToObstacle)
        {
            //目标旋转量 = 障碍物法线的反方向normal
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHitInfo.normal);
        }

        //如果需要匹配位置，才会计算匹配的位置
        if (enableTargetMatching)
        {
            //heightHitInfo 是 从击中点垂直方向发射的射线 向下击中障碍物的检测信息
            MatchPosition = hitData.heightHitInfo.point;
        }
        Debug.Log("障碍物的高度"+hitData.heightHitInfo.point.y);
        return true;

    }
    //外部可访问的属性
    public string AnimName => animName;
    public bool RotateToObstacle => rotateToObstacle;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;


}
