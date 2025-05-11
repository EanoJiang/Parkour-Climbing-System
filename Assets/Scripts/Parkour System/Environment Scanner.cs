using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header ("向前发送的射线相关参数")]
    //y轴(竖直方向)偏移量
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);
    //长度
    [SerializeField] float forwardRayLength = 0.8f;
    //障碍物层
    [SerializeField] LayerMask obstacleLayer;
    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();
        //让射线从膝盖位置开始发送
        //射线的起始位置 = 角色位置 + 一个偏移量
        var forwardOrigin = transform.position + forwardRayOffset;
        //是否击中障碍物
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward,
                                    out hitData.forwardHit, forwardRayLength, obstacleLayer);
        //调试用的射线
        //第二个参数dir：Direction and length of the ray.
        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength,
                (hitData.forwardHitFound)? Color.red : Color.white);
        return hitData;
    }
}

public struct ObstacleHitData
{
    //是否击中障碍物
    public bool forwardHitFound;
    //用来存射线检测的信息
    public RaycastHit forwardHit;
}
