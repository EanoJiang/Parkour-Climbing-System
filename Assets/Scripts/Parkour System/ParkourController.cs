using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    EnvironmentScanner environmentScanner;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    // Update is called once per frame
    private void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();
        if (hitData.forwardHitFound)
        {
            //�����ã���ӡ�ϰ�������
            Debug.Log("�ҵ��ϰ���"+ hitData.forwardHit.transform.name);
        }
    }
}
