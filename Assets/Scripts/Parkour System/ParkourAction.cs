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
        //��ȡ��ǰ���ϰ���߶� = ���е��y������ - ��ҵ�y������
        float height = hitData.heightHitInfo.point.y - player.position.y;
        //ֻ������������ڲŻ᷵��true
        if(height < minHeigth || height > maxHeigth)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    //�ⲿ�ɷ��ʵĶ�������
    public string AnimName => animName;
}
