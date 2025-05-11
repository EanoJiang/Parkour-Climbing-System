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
            //调试用：打印障碍物名称
            Debug.Log("找到障碍："+ hitData.forwardHit.transform.name);
        }
    }
}
