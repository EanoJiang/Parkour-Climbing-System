using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 和ParkourAction一样，可以通过右键菜单新建脚本相应的对象
[CreateAssetMenu(menuName = "Parkour System/Custom Actions/New Vault Action")]
public class VaultAction : ParkourAction
{
    public override bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        // 虚函数原有逻辑不变
        if(!base.CheckIfPossible(hitData, player)){
            return false;
        }
        // 增加额外的检查条件
        //击中点从全局坐标空间转到Fence上的局部坐标空间
        var hitPointFence = hitData.forwardHitInfo.transform.InverseTransformPoint(hitData.forwardHitInfo.point);
        if( (hitPointFence.z < 0 && hitPointFence.x < 0) || (hitPointFence.z > 0 && hitPointFence.x > 0) ){
            //左边沿，镜像，跟踪右手
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;

        }
        else{
            //右边沿，不镜像，跟踪左手
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }
        return true;
    }
}
