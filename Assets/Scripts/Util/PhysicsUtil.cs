using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil
{
    /// <summary>
    /// 三条射线同步检测
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="spacing"></param>间距
    /// <param name="transform"></param>方向
    /// <param name="hits"></param>
    /// <param name="distance"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool ThreeRaycast(Vector3 origin, Vector3 direction,
                                float spacing, Transform transform,
                                out List<RaycastHit> hits, float distance, LayerMask layer,
                                bool debugDraw = false)
    {
        bool centerHitFound = Physics.Raycast(origin, direction, out RaycastHit centerHit, distance, layer);
        bool leftHitFound = Physics.Raycast(origin - transform.right * spacing, direction, out RaycastHit leftHit, distance, layer);
        bool rightHitFound = Physics.Raycast(origin + transform.right * spacing, direction, out RaycastHit rightHit, distance, layer);

        //击中对象列表
        hits = new List<RaycastHit>() { centerHit, leftHit, rightHit };

        //只要一条射线命中，就认为命中
        bool hitFound = centerHitFound || leftHitFound || rightHitFound;
        //如果要显示调试射线
        if (debugDraw)
        {

            Debug.DrawLine(origin, centerHit.point, Color.red);
            Debug.DrawLine(origin - transform.right * spacing, leftHit.point, Color.red);
            Debug.DrawLine(origin + transform.right * spacing, rightHit.point, Color.red);

        }
        return hitFound;

    }

}
