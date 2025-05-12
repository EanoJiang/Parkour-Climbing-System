using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] string animName;
    [SerializeField] float minHeigth;
    [SerializeField] float maxHeigth;

    public bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        //获取面前的障碍物高度 = 击中点的y轴坐标 - 玩家的y轴坐标
        float height = hitData.heightHitInfo.point.y - player.position.y;
        //只有在这个区间内才会返回true
        if(height < minHeigth || height > maxHeigth)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    //外部可访问的动画名称
    public string AnimName => animName;
}
