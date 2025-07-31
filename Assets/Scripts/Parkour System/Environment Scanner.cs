using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;  //  .Where() extension method
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("障碍物检测——向前发送的射线相关参数")]
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
    //悬崖攀岩石层
    [SerializeField] LayerMask climbLedgeLayer;


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
                (hitData.forwardHitFound) ? Color.red : Color.white);

        //如果击中，则从击中点上方高度heightRayLength向下发射的射线
        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHitInfo.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                                    out hitData.heightHitInfo, heightRayLength, obstacleLayer);
            //调试用的射线
            //第二个参数dir：Direction and length of the ray.
            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength,
                    (hitData.heightHitFound) ? Color.red : Color.white);
        }

        return hitData;
    }

    /// <summary>
    /// 攀崖石检测
    /// </summary>
    /// <param name="dir"></param>角色朝向
    /// <param name="ledgeHit"></param>检测信息
    /// <returns></returns>
    public bool ClimbLedgeCheck(Vector3 dir,out RaycastHit ledgeHit){
        ledgeHit = new RaycastHit();
        if(dir == Vector3.zero){
            return false;
        }
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 offset = Vector3.up * 0.15f;
        //在人物朝向上循环发射多条平行检测射线
        foreach(int i in Enumerable.Range(0, 10)){
            Debug.DrawRay(origin + offset * i, dir, Color.white);
            if(Physics.Raycast(origin + offset * i, dir, out RaycastHit hit, 0.5f, climbLedgeLayer)){
                ledgeHit = hit;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检测当前位置是否有边沿攀岩架
    /// </summary>
    /// <param name="ledgeHit"></param>
    /// <returns></returns>
    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        //out修饰的参数必须要先初始化
        ledgeHit = new RaycastHit();
        Vector3 origin = transform.position + Vector3.down * 0.1f + transform.forward * 2f;
        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, 3f, climbLedgeLayer))
        {   
            ledgeHit = hit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检测是否在悬崖边缘
    /// </summary>
    /// <param name="moveDir"></param>
    /// <param name="ledgeHitData"></param>
    /// <returns></returns>
    /// out关键字需要在方法内部初始化
    public bool ObstacleLedgeCheck(Vector3 moveDir, out LedgeHitData ledgeHitData)
    {
        //用来存悬崖边缘检测相关的信息
        ledgeHitData = new LedgeHitData();

        //只有移动才会检测Ledge
        if (moveDir == Vector3.zero)
            return false;

        //起始位置向前偏移量
        float originOffset = 0.5f;
        //检测射线的起始位置
        var origin = transform.position + moveDir * originOffset + Vector3.up;    //起始位置不要在脚底，悬崖和和脚在同一高度，可能会检测不到，向上偏移一些
        //射线向下发射是否击中：击中点在地面位置，赋值给hitGround
        if (PhysicsUtil.ThreeRaycast(origin, Vector3.down, 0.25f, transform,
                out List<RaycastHit> hitsGround, ledgeRayLength, obstacleLayer, true))
        {
            //有效击中返回值列表：检查hitsGround里的所有击中信息hit
            //height：计算当前位置高度 = 角色位置高度 - 击中点高度
            //超过这个悬崖高度阈值ledgeHeightThreshold，才会认为是悬崖边缘
            var validHits = hitsGround.Where(hit => transform.position.y - hit.point.y > ledgeHeightThreshold).ToList();
            //只要有一个有效击中，就认为是悬崖边缘
            if (validHits.Count > 0)
            {
                #region 悬崖边沿竖直表面检测——悬崖边沿移动限制机制需要用到ledgeHitData.hitSurface的属性，播放JumpDown动画时判定需要用到ledgeHitData.angle和ledgeHitData.height
                // 射线起始位置：脚底向前moveDir再向下偏移一些
                var surfaceRayOrigin = validHits[0].point;
                surfaceRayOrigin.y = transform.position.y - 0.1f;
                // 射线是否击中：击中点在悬崖竖直表面，赋值给hitSurface
                if (Physics.Raycast(surfaceRayOrigin, transform.position - surfaceRayOrigin, out RaycastHit hitSurface, 2f, obstacleLayer))
                {
                    Debug.DrawLine(surfaceRayOrigin, transform.position, Color.cyan);
                    //计算当前位置高度 = 角色位置高度 - 任一击中点高度(这三个击中点高度都是一样的)
                    float height = transform.position.y - validHits[0].point.y;
                    //多个击中点，取高度最高的点作为height
                    if (validHits.Count > 1)
                    {
                        //自动选择高度最高的点作为height
                        height = validHits.Max(validHit => transform.position.y - validHit.point.y);
                    }
                    //计算当前位置与悬崖表面法线的夹角
                    ledgeHitData.angle = Vector3.Angle(transform.forward, hitSurface.normal);
                    ledgeHitData.height = height;
                    ledgeHitData.hitSurface = hitSurface;

                    return true;
                }
                #endregion
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

public struct LedgeHitData
{
    public float angle;
    public float height;
    public RaycastHit hitSurface;

}