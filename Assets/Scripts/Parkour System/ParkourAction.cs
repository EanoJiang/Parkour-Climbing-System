using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Parkour Action")]
public class ParkourAction : ScriptableObject
{
    //动画名称
    [SerializeField] string animName;
    //对应的障碍物Tag
    [SerializeField] string obstacleTag;

    [Header("高度区间")]
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;

    [Header ("自主勾选该动作是否需要转向障碍物")]
    [SerializeField] bool rotateToObstacle;
    [Header("动作播放后的延迟")]
    [SerializeField] float actionDelay;
    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching = true;
    [SerializeField] protected AvatarTarget matchBodyPart;  //在内部和子类可访问
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPositionXYZWeight = new Vector3(0, 1, 0);


    //目标旋转量
    public Quaternion TargetRotation { get; set; }
    //匹配的位置
    public Vector3 MatchPosition { get; set; }
    //动作镜像
    public bool Mirror { get; set; }

    //动作执行前的检查————这是一个虚函数，在子类中可覆盖
    //主要是找false
    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        //障碍物Tag
        //如果Tag填写了字段且不匹配，false
        if(!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHitInfo.collider.tag != obstacleTag){
            return false;
        }
        //高度Tag
        //如果高度不在区间内，false
        //获取面前的障碍物高度 = 击中点上方一定高度的y轴坐标 - 玩家的y轴坐标
        float height = hitData.heightHitInfo.point.y - player.position.y;
        if(height < minHeight || height > maxHeight)
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
    public Vector3 MatchPositionXYZWeight => matchPositionXYZWeight;
    public float ActionDelay => actionDelay;    
}
