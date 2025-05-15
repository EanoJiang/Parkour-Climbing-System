using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header ("障碍物检测——向前发送的射线相关参数")]
    //y轴(竖直方向)偏移量
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);
    //长度
    [SerializeField] float forwardRayLength = 0.8f;
    //从击中点向上发射的射线的高度
    [SerializeField] float heightRayLength = 5f;

    [Header("悬崖Ledge检测——向下发送的射线相关参数")]
    //向下发射的射线的长度
    [SerializeField] float ledgeRayLength = 10f;
    //悬崖的高度阈值
    [SerializeField] float ledgeHeightThreshold = 0.75f;

    [Header("LayerMask")]
    //障碍物层
    [SerializeField] LayerMask obstacleLayer;

    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();
        //让射线从膝盖位置开始发送
        //射线的起始位置 = 角色位置 + 一个偏移量
        var forwardOrigin = transform.position + forwardRayOffset;
        //射线向前发送是否击中障碍物：击中点在障碍物上，赋值给hitData.forwardHitInfo
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward,
                                    out hitData.forwardHitInfo, forwardRayLength, obstacleLayer);
        //调试用的射线
        //第二个参数dir：Direction and length of the ray.
        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength,
                (hitData.forwardHitFound)? Color.red : Color.white);
        
        //如果击中，则从击中点上方高度heightRayLength向下发射的射线
        if(hitData.forwardHitFound){
            var heightOrigin = hitData.forwardHitInfo.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin,Vector3.down, 
                                    out hitData.heightHitInfo, heightRayLength, obstacleLayer);
            //调试用的射线
            //第二个参数dir：Direction and length of the ray.
            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength,
                    (hitData.heightHitFound)? Color.red : Color.white);
        }
        
        return hitData;
    }

    //检测是否在悬崖边缘
    public bool LedgeCheck(Vector3 moveDir)
    {
        //只有移动才会检测Ledge
        if (moveDir == Vector3.zero)
            return false;

        //起始位置向前偏移量
        var originOffset = 0.5f;
        //检测射线的起始位置
        var origin = transform.position + moveDir * originOffset + Vector3.up;    //起始位置不要在脚底，悬崖和和脚在同一高度，可能会检测不到，向上偏移一些
        //射线向下发射是否击中：击中点在地面位置，赋值给hitGround
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitGround, ledgeRayLength, obstacleLayer))
        {
            //调试用的射线
            Debug.DrawRay(origin, Vector3.down * ledgeRayLength, Color.green);
            //计算当前位置高度 = 角色位置高度 - 击中点高度
            float height = transform.position.y - hitGround.point.y;
            //超过这个悬崖高度阈值，才会认为是悬崖边缘
            if (height > ledgeHeightThreshold)
            {
                return true;
            }
        }
        return false;
    }
}

public struct ObstacleHitData
{
    #region 从角色膝盖出发的向前射线检测相关
    //是否击中障碍物
    public bool forwardHitFound;
    //用来存射线检测的信息
    public RaycastHit forwardHitInfo;
    #endregion
    #region 从击中点垂直方向发射的射线检测相关
    public bool heightHitFound;
    //用来存射该射线向下击中障碍物的检测信息
    public RaycastHit heightHitInfo;

    #endregion

}
