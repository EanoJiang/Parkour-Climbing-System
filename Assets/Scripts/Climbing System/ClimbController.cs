using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    EnvironmentScanner envScanner;
    public bool IsOnClimbLedge{ get; private set; }
    void Awake()
    {
      envScanner = GetComponent<EnvironmentScanner>();  
    }

    void Update()
    {
        if(Input.GetButton("Jump")){
            IsOnClimbLedge = envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit);
            if(IsOnClimbLedge){
                Debug.Log("Climbing on ledge");
            }
        }
    }
}
